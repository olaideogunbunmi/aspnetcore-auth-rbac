using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoleBasedAuthenticationApi.Models
{
    public class ApplicationUser : IdentityUser 
    {
        [PersonalData]
        public string PublicId { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}






//those new property will be added as extra in AspNetUsers in DB