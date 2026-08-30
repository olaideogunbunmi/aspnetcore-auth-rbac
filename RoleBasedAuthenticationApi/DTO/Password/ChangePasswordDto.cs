using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Password
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; init; }

        [Required]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordResult
    {
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
        public PasswordChangeFailure? Failure { get; init; }
    }

    public enum PasswordChangeFailure
    {
        UserNotFound
    }
}
