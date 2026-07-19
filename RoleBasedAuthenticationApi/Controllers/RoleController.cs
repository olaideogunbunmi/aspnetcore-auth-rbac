using RoleBasedAuthenticationApi.DTO.Role;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace RoleBasedAuthenticationApi.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RoleCreatedDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RoleCreatedDto>> CreateRole(CreateRoleDto dto)
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

            return CreatedAtRoute(routeName: "getRole", routeValues: new {name = result.Name }, value: new RoleCreatedDto { RoleName = result.Name!});
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetRolesAsync();

            return Ok(roles);
        }


        [HttpGet]
        [Route("{name}", Name = "getRole")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        }


        [HttpPatch]
        [Route("{name}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateRole(string name, JsonPatchDocument<UpdateRoleDto> patchDocument)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input",
                    detail: "Role name must be provided"
                    );
            }

            if (patchDocument is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input",
                    detail: "Patch document must be provided"
                    );
            }

            var result = await _roleService.UpdateRoleAsync(name, patchDocument);

            if (!result.IsSuccess)
            {
                return result.Failure switch
                {
                    UpdateFailure.NotFound => Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Role Not Found",
                        detail: "The Role does not exist"
                        ),
                    UpdateFailure.DuplicateName => Problem(
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Duplicate role name",
                        detail: "A role with that name already exists"
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


        [HttpDelete]
        [Route("{name}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        }


        [HttpGet]
        [Route("{name}/users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<UserDetailsDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetRoleUsers(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid input",
                    detail: "Valid role name must be provided"
                    );
            }

            var result = await _roleService.GetRoleUsersAsync(name);

            if (!result.IsSuccess)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Role cannot be found"
                    );
            }

            return Ok(result.Users);
        }
    }
}
