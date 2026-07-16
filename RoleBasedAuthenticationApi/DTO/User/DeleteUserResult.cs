namespace RoleBasedAuthenticationApi.DTO.User
{
    public class DeleteUserResult
    {
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
    }
}
