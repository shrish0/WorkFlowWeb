using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WorkFlowWeb.Models
{
    public class ApplicationUser: IdentityUser
    {

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string ApplicationUserId { get; set; }

    }
}
