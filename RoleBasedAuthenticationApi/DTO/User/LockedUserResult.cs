namespace RoleBasedAuthenticationApi.DTO.User
{
    public class LockedUserResult
    {
        public bool IsSuccess { get; init; }
        public UserLockoutStatus? Failure { get; init; }
        public List<string> Errors { get; init; } = [];
    }

    public enum UserLockoutStatus
    {
        UserNotFound
    }
}
