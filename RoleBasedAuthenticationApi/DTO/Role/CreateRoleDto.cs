using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Role
{
    public class CreateRoleDto
    {
        [Required]
        public string Name {  get; set; } 
    }
}
