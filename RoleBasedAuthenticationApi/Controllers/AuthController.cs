using RoleBasedAuthenticationApi.DTO.Auth;
using RoleBasedAuthenticationApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase 
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(RegisterDto dto)//Is RESTFUL
        {
            var result = await _authService.RegisterAsync(dto);

            if (result.IsSuccess is false)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Registration failed",
                    detail: string.Join(" ", result.Errors)
                    );
            }

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    LoginResultType.UserNotFound => Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "User cannot be found"
                    ),

                    LoginResultType.AccountLocked => Problem(
                    statusCode: StatusCodes.Status423Locked,
                    title: "Account suspended",
                    detail: "Your account is temporary locked"
                    ),

                    _ => Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Authentication failed",
                    detail: "Incorrect email or password"
                    )
                };
            }

            return Ok(result.Token);
        }
    }//HTTP STANDARD
}
