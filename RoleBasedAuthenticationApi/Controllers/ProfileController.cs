using RoleBasedAuthenticationApi.DTO.Claim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly static HashSet<string> excludedClaims = new HashSet<string>
        {
            "jti",
            "nbf",
            "exp",
            "iat",
            "iss",
            "aud"
        };

        [HttpGet]
        public ActionResult<IEnumerable<UserClaimDto>> GetProfile()
        {
            //var id = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            //var email = User.FindFirstValue(JwtRegisteredClaimNames.Email)!;
            //var name = User.FindFirstValue(JwtRegisteredClaimNames.Name)!;
            //var role = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();


            var claims = User.Claims.Where(c => !excludedClaims.Contains(c.Type)).Select(c => new UserClaimDto { Type = c.Type, Value = c.Value}).ToList();

            return Ok(claims);


            //This endpoint reflects the token snapshot at login time — not the current database state. If an admin updates the user's role after they logged in, this endpoint still returns the old role until the user logs out and gets a fresh token.
        }
    }
}
