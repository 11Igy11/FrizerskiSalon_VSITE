using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FrizerskiSalon_VSITE.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string Name { get; set; } 
    }
}
