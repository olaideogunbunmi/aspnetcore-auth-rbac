using RoleBasedAuthenticationApi.DTO.Claim;
using RoleBasedAuthenticationApi.DTO.Profile;
using RoleBasedAuthenticationApi.DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/profile")]
    [ApiController]
    //[Authorize]
    [AllowAnonymous]
    public class ProfileController : ControllerBase
    {
        [HttpGet]
        public ActionResult<UserProfileDto> GetProfile()
        {
            var id = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var email = User.FindFirstValue(claimType: ClaimTypes.Email);
            var name = User.FindFirstValue(claimType: ClaimTypes.Name);         
            var role = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();


            var standardClaimType = new HashSet<string>
            {
                JwtRegisteredClaimNames.Sub,
                ClaimTypes.Email,
                ClaimTypes.Name,
                ClaimTypes.Role
            };

            var customClaims = User.Claims.Where(c => !standardClaimType.Contains(c.Type)).Select(c => new UserClaimDto { Type = c.Type, Value = c.Value }).ToList();


            return Ok(new UserProfileDto
            {
                Id = id,
                Email = email,
                Name = name,
                Role = role,
                CustomClaims = customClaims,
            });


            //This endpoint reflects the token snapshot at login time — not the current database state. If an admin updates the user's role after they logged in, this endpoint still returns the old role until the user logs out and gets a fresh token.
        }
    }
}
