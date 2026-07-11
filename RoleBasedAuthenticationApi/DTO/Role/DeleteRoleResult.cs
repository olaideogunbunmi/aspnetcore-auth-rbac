using RoleBasedAuthenticationApi.Models;

namespace RoleBasedAuthenticationApi.DTO.Role
{
    public class DeleteRoleResult
    {
        public bool IsSuccess { get; init; }
        public DeleteRoleFailure? Failure { get; init; }
        public List<string> Errors { get; init; } = [];
        
    }
    public enum DeleteRoleFailure //rename this
    {
        NotFound,
        DeleteFailed
    }
}
