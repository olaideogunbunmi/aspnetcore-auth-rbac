namespace RoleBasedAuthenticationApi.DTO.Role
{
    public class CreateRoleResult
    {
        public bool IsSuccess { get; init; }
        public RoleCreateFailure? Failure { get; init; }
        public List<string> Errors { get; init; } = [];
    }

    public enum RoleCreateFailure
    {
        DuplicateRole
    }
}
