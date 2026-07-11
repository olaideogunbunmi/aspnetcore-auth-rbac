using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationApi.DTO.Claim
{
    public class ClaimDto //input claim dto
    {
        [Required]
        public string Type { get; init; }

        [Required]
        public string Value { get; init; }
    }

    
}
