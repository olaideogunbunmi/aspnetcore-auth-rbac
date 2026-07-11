namespace RoleBasedAuthenticationApi.DTO.User
{
    public class UnlockedUserResult
    {
        public bool IsSuccess { get; init; }
        public UserLockoutStatus? Failure { get; init; }
        public List<string> Errors { get; init; } = [];
    }
}
