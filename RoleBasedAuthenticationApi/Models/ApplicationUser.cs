using Microsoft.AspNetCore.Identity;

namespace RoleBasedAuthenticationApi.Models
{
    public class ApplicationUser : IdentityUser 
    {
        [PersonalData]
        public string PublicId { get; init; }
        public string FullName { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}






//those new property will be added as extra in AspNetUsers in DB