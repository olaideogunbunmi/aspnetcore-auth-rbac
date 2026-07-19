using AutoMapper;
using RoleBasedAuthenticationApi.DTO.Role;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;


namespace RoleBasedAuthenticationApi.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mapper = mapper;

        }

        public async Task<CreateRoleResult> CreateRoleAsync(CreateRoleDto dto)
        {
            var roleExist = await _roleManager.RoleExistsAsync(dto.Name);

            if (roleExist)
            {
                return new CreateRoleResult
                {
                    IsSuccess = false,
                    Failure = RoleCreateFailure.DuplicateRole
                };
            }

            var role = new IdentityRole(dto.Name);

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return new CreateRoleResult
                {
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            return new CreateRoleResult
            {
                IsSuccess = true,
                Name = dto.Name
            };
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            return await _mapper.ProjectTo<RoleDto>(_roleManager.Roles).ToListAsync();
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string name)
        {
            return await _mapper.ProjectTo<RoleDto>(_roleManager.Roles.Where(r => r.Name == name)).FirstOrDefaultAsync();
        }

        public async Task<UpdateRoleResult> UpdateRoleAsync(string name, JsonPatchDocument<UpdateRoleDto> patchDocument)
        {
            var role = await _roleManager.FindByNameAsync(name);

            if (role == null)
            {
                return new UpdateRoleResult
                {
                    IsSuccess = false,
                    Failure = UpdateFailure.NotFound
                };
            }

            var roleDto = _mapper.Map<UpdateRoleDto>(role);

            patchDocument.ApplyTo(roleDto);

            if (roleDto.NewName == role.Name)
            {
                return new UpdateRoleResult
                {
                    IsSuccess = false,
                    Failure = UpdateFailure.DuplicateName
                };
            }

            role.Name = roleDto.NewName;

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "DuplicateRoleName"))
                {
                    return new UpdateRoleResult
                    {
                        IsSuccess = false,
                        Failure = UpdateFailure.DuplicateName,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                return new UpdateRoleResult
                {
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };

            }

            return new UpdateRoleResult
            {
                IsSuccess = true,
            };

        }

        public async Task<DeleteRoleResult> DeleteRoleAsync(string name)
        {
            var role = await _roleManager.FindByNameAsync(name);

            if (role == null)
            {
                return new DeleteRoleResult
                {
                    IsSuccess = false,
                    Failure = DeleteRoleFailure.NotFound
                };
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return new DeleteRoleResult
                {
                    IsSuccess = false,
                    Failure = DeleteRoleFailure.DeleteFailed,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            return new DeleteRoleResult
            {
                IsSuccess = true
            };

        }

        public async Task<UsersRetrieveResult> GetRoleUsersAsync(string name)
        {
            var role = await _roleManager.FindByNameAsync(name);

            if (role == null)
            {
                return new UsersRetrieveResult
                {
                    IsSuccess = false,
                    Failure = RetrieveFailure.RoleNotFound
                };
            }

            var users = await _userManager.GetUsersInRoleAsync(name);

            return new UsersRetrieveResult
            {
                Users = _mapper.Map<List<UserDetailsDto>>(users),
                IsSuccess = true
            };
        }
    }
}
