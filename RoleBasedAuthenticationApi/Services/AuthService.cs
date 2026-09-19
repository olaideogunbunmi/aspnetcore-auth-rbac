using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RoleBasedAuthenticationApi.Configuration;
using RoleBasedAuthenticationApi.Data;
using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.DTO.Password;
using RoleBasedAuthenticationApi.DTO.Token;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace RoleBasedAuthenticationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWTSettings _jwtSettings;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailServices _emailServices;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, ILogger<AuthService> logger, IEmailServices emailServices, IOptions<JWTSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _emailServices = emailServices;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser()
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "DuplicateEmail"))
                {
                    return new RegisterResult
                    {
                        IsSuccess = false,
                        Failure = RegisterFailure.DuplicateEmail,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                if (result.Errors.Any(e => e.Code == "PasswordTooShort" || e.Code == "PasswordRequiresDigit" || e.Code == "PasswordRequiresUpper"))
                {
                    return new RegisterResult
                    {
                        IsSuccess = false,
                        Failure = RegisterFailure.ValidationFailed,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                return new RegisterResult
                {
                    IsSuccess = false,
                    Failure = RegisterFailure.UnexpectedError,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            return new RegisterResult
            {
                IsSuccess = true,
                User = new UserRegisteredDto
                {
                    Id = user.PublicId,
                    Name = user.FullName,
                    Email = user.Email
                }
            };
        }

        public async Task<LoginResult> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Failure = LoginResultType.UserNotFound
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);


            if (result.IsLockedOut)
            {
                var lockOut = await _userManager.GetLockoutEndDateAsync(user);

                if (lockOut.HasValue && lockOut.Value < DateTimeOffset.MaxValue)
                {
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                }

                return new LoginResult
                {
                    IsSuccess = false,
                    Failure = LoginResultType.AccountLocked
                };
            }

            if (!result.Succeeded)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Failure = LoginResultType.InvalidCredentials
                };
            }

            var accessToken = await GenerateJwtToken(user);
            var rawRefreshToken = GenerateRefreshToken();

            await RotateRefreshTokenOnLoginAsync(user, rawRefreshToken);

            return new LoginResult
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken
            };
        }

        public async Task<RefreshTokenResult> RefreshTokenAsync(string rawToken)
        {
            string hashedToken = HashToken(rawToken);

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hashedToken);

            if (token == null)
            {
                return new RefreshTokenResult
                {
                    IsSuccess = false,
                    Failure = TokenFailureType.Invalid
                };
            }

            if (token.Revoked)
            {
                //await RevokeAllUserRefreshTokensAsync(token.UserId, token.TokenHash);
                await RevokeAllUserRefreshTokensAsync(token.UserId, $"Refresh token reuse detected. Token: {token.TokenHash}");

                return new RefreshTokenResult
                {
                    IsSuccess = false,
                    Failure = TokenFailureType.ReuseDetected
                };
            }

            if (DateTimeOffset.UtcNow > token.ExpiredAt)
            {
                return new RefreshTokenResult
                {
                    IsSuccess = false,
                    Failure = TokenFailureType.Expired
                };
            }

            var user = await _userManager.FindByIdAsync(token.UserId);

            if (user == null)
            {
                return new RefreshTokenResult
                {
                    IsSuccess = false,
                    Failure = TokenFailureType.UserNotFound
                };
            }

            var newJwtToken = await GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            var newTokenEntity = new RefreshToken
            {
                UserId = token.UserId,
                TokenHash = HashToken(newRefreshToken),
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
                Revoked = false
            };

            //Token Rotation and Revocation

            token.Revoked = true;
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.ReplaceByToken = newTokenEntity;


            await _context.RefreshTokens.AddAsync(newTokenEntity);
            await _context.SaveChangesAsync();

            return new RefreshTokenResult
            {
                IsSuccess = true,
                AccessToken = newJwtToken,
                RefreshToken = newRefreshToken
            };
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
                //new Claim(ClaimTypes.NameIdentifier, user.PublicId.ToString()), //for .NET

                //prevent tokens having same payload and signature - though nearly impossble for same signature to be generated

                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName!)

            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //reporting from/to AspNetUserClaims
            var customClaim = await _userManager.GetClaimsAsync(user);

            claims.AddRange(customClaim);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            byte[] randomString = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomString);
        }

        private string HashToken(string rawToken)
        {
            byte[] refreshTokenBytes = Encoding.UTF8.GetBytes(rawToken);
            byte[] refreshTokenHash = SHA512.HashData(refreshTokenBytes);

            return Convert.ToBase64String(refreshTokenHash);
        }

        private async Task RotateRefreshTokenOnLoginAsync(ApplicationUser user, string rawRefreshToken)
        {
            var hashedRefreshToken = HashToken(rawRefreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hashedRefreshToken,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
                Revoked = false,
            };

            //Token Rotation and Revocation

            var userOldRefreshToken = await _context.RefreshTokens.OrderByDescending(u => u.CreatedAt).FirstOrDefaultAsync(x => x.UserId == user.Id);


            if (userOldRefreshToken != null)
            {
                userOldRefreshToken.Revoked = true;
                userOldRefreshToken.RevokedAt = DateTimeOffset.UtcNow;
                userOldRefreshToken.ReplaceByToken = refreshTokenEntity;
            }

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();
        }

        private async Task RevokeAllUserRefreshTokensAsync(string userId, string reason)
        {
            var tokens = await _context.RefreshTokens.Where(x => x.UserId == userId && !x.Revoked).ToListAsync();

            foreach (var tokenEntity in tokens)
            {
                tokenEntity.Revoked = true;
                tokenEntity.RevokedAt = DateTimeOffset.UtcNow;
            }

            if (tokens.Count > 0)
            {
                _context.RefreshTokens.UpdateRange(tokens);
                await _context.SaveChangesAsync();
            }

            _logger.LogWarning("All refresh tokens have been revoked: User: {UserId}, Reason: {Reason}", userId, reason);
        }

        public async Task LogoutAsync(string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PublicId == id);

            if (user == null)
            {
                return;
            }

            var userRefreshToken = await _context.RefreshTokens.Where(x => x.UserId == user.Id && !x.Revoked).FirstOrDefaultAsync();

            if (userRefreshToken != null)
            {
                userRefreshToken.Revoked = true;
                userRefreshToken.RevokedAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            try
            {
                await _emailServices.SendPasswordResetEmailAsync(user.Email!, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to user {UserId}", user.Id);
            }
        }

        public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Failure = ResetFailure.InvalidTokenOrEmail
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.ResetToken, dto.NewPassword);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "InvalidToken"))
                {
                    return new ResetPasswordResult
                    {
                        IsSuccess = false,
                        Failure = ResetFailure.InvalidTokenOrEmail
                    };
                }                

                return new ResetPasswordResult
                {
                    IsSuccess = false,
                    Failure = ResetFailure.PasswordPolicyViolation,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };              
            }

            await RevokeAllUserRefreshTokensAsync(user.Id, "Password reset");

            return new ResetPasswordResult
            {
                IsSuccess = true
            };
        }

        public async Task<ChangePasswordResult> ChangePasswordAsync(string id, ChangePasswordDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PublicId == id);

            if (user == null)
            {
                return new ChangePasswordResult
                {
                    IsSuccess = false,
                    Failure = PasswordChangeFailure.InvalidUser
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
                {
                    return new ChangePasswordResult
                    {
                        IsSuccess = false,
                        Failure = PasswordChangeFailure.IncorrectCurrentPassword
                    };
                }

                return new ChangePasswordResult
                {
                    IsSuccess = false,
                    Failure = PasswordChangeFailure.PasswordPolicyViolation,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            await RevokeAllUserRefreshTokensAsync(user.Id, "Password change");

            return new ChangePasswordResult
            {
                IsSuccess = true
            };

        }
        public static void VerifyEmail()
        {

        }
    }
}
