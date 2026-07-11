using RoleBasedAuthenticationApi.DTO.Claim;
using RoleBasedAuthenticationApi.DTO.Role;
using RoleBasedAuthenticationApi.DTO.User;
using RoleBasedAuthenticationApi.Models;
using System.Security.Claims;

namespace RoleBasedAuthenticationApi.Interfaces
{
    public interface IUserService
    {
        Task<UserDetailsDto?> GetUserProfileAsync(string email);
        Task<List<UserDetailsDto>> GetUsersAsync();
        Task<UpdateUserResult?> UpdateUserAsync(string email, UpdateUserDto updateDto);

        Task<DeleteUserResult?> DeleteUserAsync(string email);
        Task<AssignRoleResult> AssignRoleAsync(string email, AssignRoleDto dto);
        
        Task<UserRoleRemoveResult> RemoveRoleFromUserAsync(string email, string name);
        Task<GetUserRolesResult> GetUserRolesAsync(string email);
        Task<GetUserClaimsResult> GetUserClaimsAsync(string id);

        Task<AddCLaimResult> AddUserClaimsAsync(string id, ClaimDto dto);
        Task<RemoveClaimResult> RemoveUserClaimsAsync(string id, string type, string value);
        Task<LockedUserResult> DisableUserAsync(string id);
        Task<UnlockedUserResult> EnableUserAsync(string id);

    }
}
