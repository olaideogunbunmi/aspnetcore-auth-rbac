using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Password
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; init; }

        [Required]
        public string ResetToken { get; init; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; init; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password does not match")]
        public string ConfirmPassword { get; init; }
    }

    public class ResetPasswordResult
    {
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
        public ResetFailure? Failure { get; init; }
    }

    public enum ResetFailure
    {
        InvalidTokenOrEmail,
        PasswordPolicyViolation
    }
}
