using System.ComponentModel.DataAnnotations;

namespace WorkFlowWeb.ViewModels
{
    public class UserViewModel
    {
        public int? Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }

        [Phone]
        [StringLength(10)]
        public string PhoneNumber { get; set; }

        public string Role { get; set; }
    }

}
