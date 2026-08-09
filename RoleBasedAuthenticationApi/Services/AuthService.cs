using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using RoleBasedAuthenticationApi.Data;
using Microsoft.EntityFrameworkCore;


namespace RoleBasedAuthenticationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        
        
        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _context = context;
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
            var rawRefreshToken = await GenerateRefreshToken();
            var hashedRefreshToken = await HashToken(rawRefreshToken);


            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id, //thinking of using PublicId
                TokenHash = hashedRefreshToken,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
                Revoked = false,
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new LoginResult
            {
                IsSuccess = true,
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken
            };
        }

        public async Task<string> RefreshTokenAsync(string rawToken)
        {
           string hashedToken =  await HashToken(rawToken);
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash ==  hashedToken && !x.Revoked);

            if (token == null || token.ExpiredAt < DateTimeOffset.UtcNow)
            {
                return "Invalid or expired token";
            }

            var user = await _userManager.FindByIdAsync(token.UserId);

            if (user == null)
            {
                return "User not found";
            }

            var newAccessToken = await GenerateJwtToken(user);

            return newAccessToken.ToString();

            //var newRefreshToken = await GenerateRefreshToken();
            
            //Token Rotation and Revocation
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));

            var credentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
                //new Claim(ClaimTypes.NameIdentifier, user.PublicId.ToString()), //for .NET

                //prevent tokens having same payload and signature - though nearly impossble for same signature to be generated
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),         
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName),
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
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"],
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        private async Task<string> GenerateRefreshToken()
        {
            byte[] randomString = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomString);
        }

        private async Task<string> HashToken(string rawToken)
        {
            byte[] refreshTokenBytes = Encoding.UTF8.GetBytes(rawToken);
            byte[] refreshTokenHash = SHA512.HashData(refreshTokenBytes);

            return Convert.ToBase64String(refreshTokenHash);
        }

        //public static void RefreshToken()
        //{
        //    //important due to expiration of generated token
        //    //give it a revoke flag, in case of locked account so it won;t give it refresh token while access token dies off - revocation
        //    //rotation
        //    //revocation
        //}

        public static void TokenRotation()
        {

        }
        public static void TokenRevocation()
        {

        }
        public static void Logout()
        {

        }
        public static void ForgotPassword()
        {

        }
        public static void ResetPassword()
        {

        }
        public static void ChangePassword()
        {

        }
        public static void VerifyEmail()
        {

        }
    }
}
