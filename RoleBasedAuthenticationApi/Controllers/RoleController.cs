using RoleBasedAuthenticationApi.DTO.Role;
using RoleBasedAuthenticationApi.DTO.User;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole(CreateRoleDto dto)
        {
            var result = await _roleService.CreateRoleAsync(dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    RoleCreateFailure.DuplicateRole => Problem(
                        statusCode: StatusCodes.Status409Conflict,
                    title: "Role already exists",
                    detail: "A role with that name has already been created"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Role creation failed",
                        detail: string.Join(", ", result.Errors)
                        )
                };
            }

            return Created();
        }//STANDARD


        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetRolesAsync();

            return Ok(roles);
        }//STANDARD


        [HttpGet]
        [Route("{name:alpha}")]
        public async Task<ActionResult<RoleDto>> GetRole(string name)
        {
            var result = await _roleService.GetRoleByNameAsync(name);

            if (result is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Role cannot be found"
                    );
            }

            return Ok(result);

        }//STANDARD


        [HttpPatch]
        [Route("{name}")]
        public async Task<ActionResult> UpdateRole(string name, JsonPatchDocument<UpdateRoleDto> dto) //change to patchDocu
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input",
                    detail: "Role name must be provided"
                    );
            }

            if (dto is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input",
                    detail: "Patch document must be provided"
                    );
            }

            var result = await _roleService.UpdateRoleAsync(name, dto);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    UpdateFailure.NotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Role Not Found",
                        detail: "The Role does not exist"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: string.Join(", ", result.Errors)
                        )
                };
            }

            return NoContent();
        }//STANDARD


        [HttpDelete]
        [Route("{name}")]
        public async Task<ActionResult> DeleteRole(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input",
                    detail: "Valid role name must be provided"
                    );
            }

            var result = await _roleService.DeleteRoleAsync(name);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    DeleteRoleFailure.NotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Role Not Found",
                        detail: "The Role cannot be found"
                        ),

                    _ => Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Unexpected error",
                        detail: string.Join(", ", result.Errors)
                        )
                };
            }

            return NoContent();
        }//STANDARD


        [HttpGet]
        [Route("{name}/users")]
        public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetRoleUsers(string name)
        {
            var result = await _roleService.GetRoleUsersAsync(name);

            if(result.IsSuccess is false)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Role cannot be found"
                    );
            }

            return Ok(result.Users);
        }//STANDARD
    }
}
