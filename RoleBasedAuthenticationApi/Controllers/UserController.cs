using RoleBasedAuthenticationApi.DTO.Claim;
using RoleBasedAuthenticationApi.DTO.Role;
using RoleBasedAuthenticationApi.DTO.User;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using static RoleBasedAuthenticationApi.DTO.Claim.AddCLaimResult;

namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    //[Authorize]
    [AllowAnonymous]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();

            return Ok(users);
        }


        [HttpGet]
        [Route("{id}", Name = "getuser")]
        public async Task<ActionResult<UserDetailsDto>> GetUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var user = await _userService.GetUserProfileAsync(id);

            if (user == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "The user ID cannot be found"
                    );
            }

            return Ok(user);
        }


        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<UserDetailsDto>> UpdateUser(string id, UpdateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var updated = await _userService.UpdateUserAsync(id, dto);

            if (updated == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "User ID cannot be found"
                    );
            }

            if (!updated.IsSuccess)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Update failed",
                    detail: string.Join(", ", updated.Errors)
                    );
            }
             
            return Ok(updated.User);
        }


        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var deleted = await _userService.DeleteUserAsync(id);

            if (deleted == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "User ID cannot be found"
                    );
            }

            if (!deleted.IsDeleted)
            {
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Deletion failed",
                    detail: string.Join(", ", deleted.Errors)
                    );
            }

            return NoContent();
        }


        [HttpPost]
        [Route("{id}/roles")]
        public async Task<ActionResult> AssignRole(string id, AssignRoleDto dto) 
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var assigned = await _userService.AssignRoleAsync(id, dto);

            if (!assigned.IsAssigned)
            {
                return assigned.Failure switch
                {
                    AssignFailure.UserNotFound => Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "User cannot be found"
                    ),

                    AssignFailure.RoleNotFound => Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Role cannot be found"
                    ),

                    _ => Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Role assignment failed",
                    detail: string.Join(", ", assigned.Errors)
                    )
                };                   
            }
            
            return NoContent();
        }


        [HttpDelete]
        [Route("{id}/roles/{roleName}")]
        public async Task<ActionResult> RemoveRole(string id, string roleName)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var result = await _userService.RemoveRoleFromUserAsync(id, roleName);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    RemoveFailure.UserNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: "The user cannot be found"
                        ),

                    RemoveFailure.RoleNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: "The role cannot be found"
                        ),

                     _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Role removal failed",
                        detail: string.Join(", ", result.Errors)
                        )         
                };
            }

            return NoContent();
        }


        [HttpGet]
        [Route("{id}/roles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetUserRoles(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var result = await _userService.GetUserRolesAsync(id);

            if (result.UserNotFound)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "User cannot be found"
                    );
            }

            return Ok(result.Roles);
        }


        [HttpGet]
        [Route("{id}/claims")]
        public async Task<ActionResult<IEnumerable<UserClaimDto>>> GetUserClaim(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var result = await _userService.GetUserClaimsAsync(id);

            if (result.UserNotFound)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "User not found"
                    );
            }

            return Ok(result.Claims);

        }


        [HttpPost]
        [Route("{id}/claims")]
        public async Task<ActionResult> AddClaims(string id, ClaimDto dto)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var result = await _userService.AddUserClaimsAsync(id, dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    AddClaimFailure.UserNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: "User does not exist"
                        ),

                    AddClaimFailure.ClaimAlreadyExist => Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Conflict",
                        detail: "This claim already exists for this user"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Add claim failed",
                        detail: string.Join(", ", result.Errors)
                        )
                };
            }

            return NoContent();
        }


        [HttpDelete]
        [Route("{id}/claims/{type}/{value}")]
        public async Task<ActionResult> RemoveClaims(string id, string type, string value)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input",
                    detail: "Claim type and value must be provided"
                );
            }

            var result = await _userService.RemoveUserClaimsAsync(id, type, value);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    ClaimRemoveFailure.UserNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: "User does not exist"
                        ),

                    ClaimRemoveFailure.ClaimNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not found",
                        detail: "This claim does not exist for this user"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Remove claim failed",
                        detail: string.Join(", ", result.Errors)
                        )
                };
            }

            return NoContent();

        }


        [HttpPost]
        [Route("{id}/disable")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DisableUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var result = await _userService.DisableUserAsync(id);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    UserLockoutStatus.UserNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: "User cannot be found"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: string.Join(", ", result.Errors)
                        )
                };
            }

            return NoContent();
        }


        [HttpPost]
        [Route("{id}/enable")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> EnableUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Regex.IsMatch(id, @"^\d{6}$"))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input format",
                    detail: "The user ID must be exactly 6 digits. Letters or symbols are not allowed"
                    );
            }

            var result = await _userService.EnableUserAsync(id);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    UserLockoutStatus.UserNotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Not Found",
                        detail: "User cannot be found"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: string.Join(", ", result.Errors)
                        )
                };
            }

            return NoContent();
        }
    }
}
