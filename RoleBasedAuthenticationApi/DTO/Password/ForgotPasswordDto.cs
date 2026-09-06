using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Password
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; init; }
    }  
}
