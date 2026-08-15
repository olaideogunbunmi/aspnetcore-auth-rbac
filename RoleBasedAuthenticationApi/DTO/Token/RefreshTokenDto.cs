using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Token
{
    public class RefreshTokenDto
    {
        [Required]
        public string Token {  get; init; }
    }
}
