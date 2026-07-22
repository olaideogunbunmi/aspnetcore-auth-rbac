using RoleBasedAuthenticationApi.DTO.Role;

namespace RoleBasedAuthenticationApi.DTO.User
{
    public class GetUserRolesResult
    {
        public bool UserNotFound { get; init; }
        public List<string> Roles { get; init; } = [];
    }
}
