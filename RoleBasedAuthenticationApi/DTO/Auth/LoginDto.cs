using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }

        [Required]
        public string Password { get; init; }
    }

    public class LoginResponseDto
    {
        public string Token { get; init; }
    }
}
