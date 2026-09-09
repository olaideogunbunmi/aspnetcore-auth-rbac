using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Auth
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; init; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password does not match")]
        public string ConfirmPassword { get; init; }
    }

    public class UserRegisteredDto
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
        
    }
}
