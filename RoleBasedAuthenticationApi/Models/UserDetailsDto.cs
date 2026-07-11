using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.Models
{
    public class UserDetailsDto
    {
        public string Id { get; init; }
        public string Email { get; init; }
        public string FullName { get; init; }
    }
}
