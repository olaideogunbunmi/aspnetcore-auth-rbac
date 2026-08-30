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
        public string NewPassword { get; init; } 
    }

    public class ResetPasswordResult
    {
        public bool IsSuccess { get; init; }
        public List<string> Errors { get; init; } = [];
        public ResetFailure? Failure { get; init; }
    }

    public enum ResetFailure
    {
        UserNotFound
    }
}
