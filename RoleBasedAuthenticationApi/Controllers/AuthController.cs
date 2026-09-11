using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.DTO.Password;
using RoleBasedAuthenticationApi.DTO.Token;
using RoleBasedAuthenticationApi.Interfaces;
using System.Security.Claims;


namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(typeof(UserRegisteredDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserRegisteredDto>> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    RegisterFailure.DuplicateEmail => Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Duplicate email",
                        detail: string.Join(", ", result.Errors)
                    ),

                    RegisterFailure.ValidationFailed => Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Registration failed",
                        detail: string.Join(", ", result.Errors)
                    ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: string.Join(", ", result.Errors)
                    )
                };
            }

            return CreatedAtRoute(
                routeName: "getuser",
                routeValues: new { id = result.User!.Id },
                value: result.User);
        }


        [HttpPost]
        [Route("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    LoginResultType.UserNotFound or LoginResultType.InvalidCredentials => Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication failed",
                    detail: "Incorrect email or password"
                    ),

                    LoginResultType.AccountLocked => Problem(
                    statusCode: StatusCodes.Status423Locked,
                    title: "Account suspended",
                    detail: "Your account has been locked due to multiple failed login attempts."
                    ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: "An unexpected error occurred"
                        )
                };
            }

            return Ok(new LoginResponseDto
            {
                AccessToken = result.AccessToken!,
                RefreshToken = result.RefreshToken!
            });
        }


        [HttpPost]
        [Route("refresh")]
        [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto.Token);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    TokenFailureType.Invalid => Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "invalid_grant",
                        detail: "The token is invalid or does not exist"
                        ),

                    TokenFailureType.Expired => Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "invalid_grant",
                    detail: "The token has expired or has been revoked"
                    ),

                    TokenFailureType.ReuseDetected => Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "invalid_grant",
                    detail: "The token has been revoked"
                    ),

                    TokenFailureType.UserNotFound => Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "invalid_grant",
                        detail: "This token can no longer be used"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: "An unexpected error occurred"
                        )
                };
            }

            return Ok(new RefreshTokenResponseDto
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });

        }


        [HttpPost]
        [Route("logout")]
        [Authorize] //only authenticated user can logout
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Logout()
        {
            var id = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            await _authService.LogoutAsync(id!);

            return NoContent();
        }


        [HttpPost]
        [Route("forgotpassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            //implement password policy here min lenght

            await _authService.ForgotPasswordAsync(dto);

            return Ok(new { message = "If an account with this email exists, a password reset token has been sent." });
        }


        [HttpPost]
        [Route("resetpassword")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    ResetFailure.InvalidTokenOrEmail => Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "The reset token or email address is invalid"
                        ),

                    ResetFailure.PasswordPolicyViolation => Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Password does not meet requirements",
                    detail: string.Join(", ", result.Errors)
                    ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: "An unexpected error occurred"
                        )
                };
            }

            return NoContent();
        }


        [HttpPost]
        [Route("changepassword")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var id = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var result = await _authService.ChangePasswordAsync(id!, dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    PasswordChangeFailure.InvalidUser => Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request",
                        detail: "Unable to process this request"
                        ),
                    PasswordChangeFailure.IncorrectCurrentPassword => Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Incorrect password",
                        detail: "The current password you entered is incorrect"
                        ),

                    PasswordChangeFailure.PasswordPolicyViolation => Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Password does not meet requirements",
                        detail: string.Join(", ", result.Errors)
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: "An unexpected error occurred"
                        )
                };
            }

            return NoContent();
        }
    }
}
