using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.Configuration
{
    public class JWTSettings
    {
        [Required]
        public string Key { get; init; } = string.Empty;

        [Required]
        public string Issuer { get; init; } = string.Empty;

        [Required]
        public string Audience { get; init; } = string.Empty;
    }
}
