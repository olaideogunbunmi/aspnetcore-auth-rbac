using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoleBasedAuthenticationApi.Models
{
    public class ApplicationUser : IdentityUser 
    {
        [PersonalData]
        public string PublicId { get; init; }
        public string FullName { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}






//those new property will be added as extra in AspNetUsers in DB