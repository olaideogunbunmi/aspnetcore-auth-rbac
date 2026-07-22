using RoleBasedAuthenticationApi.Models;

namespace RoleBasedAuthenticationApi.DTO.User
{
    public class UserRoleRemoveResult
    {
        public bool IsSuccess { get; init; }
        public RemoveFailure? Failure { get; init; }
        public List<string> Errors { get; init; } = [];
    }
    public enum RemoveFailure
    {
        UserNotFound,
        RoleNotFound,
        RemoveFailed,
        UserNotInRole
    }
}
