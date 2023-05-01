using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class RegisterViewModel
    {
        [Required, Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required, Display(Name = "Last name")]
        public string LastName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Phone, Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, Display(Name = "Repeat password")]
        public string RepeatPassword { get; set; }
    }
}
