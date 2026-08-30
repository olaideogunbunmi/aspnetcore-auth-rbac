using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Password
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; init; }
    }

    public class ForgotPasswordResult
    {
        public bool IsSuccess { get; init; }
        public string? ResetToken { get; init; }
    }

    public class TokenResetDto
    {
        public string? Token { get; init; } 
    }

   
}
