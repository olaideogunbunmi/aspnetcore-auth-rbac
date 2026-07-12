using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoleBasedAuthenticationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _roleManager = roleManager;
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

                    };
                }

                if (result.Errors.Any(e => e.Code == "PasswordTooShort"
                || e.Code == "PasswordRequiresDigit"
                || e.Code == "PasswordRequiresUpper"))
                {
                    return new RegisterResult
                    {
                        IsSuccess = false,
                        Failure = RegisterFailure.ValidationFailed,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }
            }

            return new RegisterResult
            {
                IsSuccess = true,
                Id = user.PublicId
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



            return new LoginResult
            {
                IsSuccess = true,
                Token = await GenerateJwtToken(user)
            };
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));

            var credentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()),
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

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static void RefreshToken()
        {
            //important
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
