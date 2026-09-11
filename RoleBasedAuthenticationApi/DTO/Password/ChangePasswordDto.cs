using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Password
{
    public class ChangePasswordDto
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; init; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; init; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; init; }
    }

    public class ChangePasswordResult
    {
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
        public PasswordChangeFailure? Failure { get; init; }
    }

    public enum PasswordChangeFailure
    {
        InvalidUser,
        IncorrectCurrentPassword,
        PasswordPolicyViolation
    }
}
