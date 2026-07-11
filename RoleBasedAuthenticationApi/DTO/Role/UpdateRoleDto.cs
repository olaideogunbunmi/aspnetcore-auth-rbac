using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Role
{
    public class UpdateRoleDto
    {
        [Required]
        public string NewName { get; set; }
    }

    public class UpdateRoleResult
    {
        public bool IsSuccess { get; init; }
        public UpdateFailure? Failure { get; init; }
        public List<string> Errors { get; init; } = [];
    }

    public enum UpdateFailure
    {
        NotFound,
        RoleUpdateFailed
    }
}
