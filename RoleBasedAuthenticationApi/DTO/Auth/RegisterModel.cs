using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Auth
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; init; }
    }

    public class UserRegisteredDto
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
        
    }
}
