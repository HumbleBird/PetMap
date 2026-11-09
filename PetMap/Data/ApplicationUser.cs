using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PetMap.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(30)]
        public string Nickname { get; set; } = string.Empty;
    }

}
