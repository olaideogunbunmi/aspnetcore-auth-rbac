namespace RoleBasedAuthenticationApi.DTO.User
{
    public class DeleteUserResult
    {
        public bool IsDeleted { get; init; }
        public List<string> Errors { get; init; } = [];
    }
}
