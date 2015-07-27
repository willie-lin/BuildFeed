using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Models.ViewModel
{
    public class RegistrationUser
    {
        [Required]
        [Display(ResourceType = typeof(Support), Name ="UserName")]
        public string UserName { get; set; }

        [Required]
        [MinLength(12)]
        [Display(ResourceType = typeof(Support), Name = "EnterPassword")]
        public string Password { get; set; }

        [Required]
        [MinLength(12)]
        [Display(ResourceType = typeof(Support), Name = "ConfirmPassword")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(Support), Name = "EmailAddress")]
        public string EmailAddress { get; set; }
    }
}