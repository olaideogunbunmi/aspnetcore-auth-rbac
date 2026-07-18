using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UserRegisteredDto), StatusCodes.Status201Created)]
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

            return CreatedAtRoute(routeName: "getuser", routeValues: new { id = result.User!.Id }, value: result.User);
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
                    detail: "Your account is temporarily locked"
                    ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: "An unexpected error occurred"
                        )
                };
            }

            return Ok(new LoginResponseDto { Token = result.Token! });
        }
    }
}
