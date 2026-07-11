using RoleBasedAuthenticationApi.DTO.Role;
using Microsoft.AspNetCore.JsonPatch;

namespace RoleBasedAuthenticationApi.Interfaces
{
    public interface IRoleService
    {
        Task<CreateRoleResult> CreateRoleAsync(CreateRoleDto role);
        Task<IEnumerable<RoleDto>> GetRolesAsync();
        Task<RoleDto?> GetRoleByNameAsync(string name);
        Task<UpdateRoleResult> UpdateRoleAsync(string name, JsonPatchDocument<UpdateRoleDto> dto);
        Task<DeleteRoleResult> DeleteRoleAsync(string roleName);
        Task<UsersRetrieveResult> GetRoleUsersAsync(string email);
    }
}
