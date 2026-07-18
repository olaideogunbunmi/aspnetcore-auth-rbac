using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Role
{
    public class CreateRoleDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s-]+$", ErrorMessage = "Role name can only contain letters, numbers, spaces, or hyphens.")]
        public string Name { get; init; } 
    }

    public class RoleCreatedDto
    {
        public string RoleName { get; init; }
    }
}
