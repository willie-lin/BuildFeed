using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Models.ViewModel
{
    public class LoginUser
    {
        [Required]
        [Display(ResourceType = typeof(Support), Name = "UserName")]
        public string UserName { get; set; }

        [Required]
        [Display(ResourceType = typeof(Support), Name = "Password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(Support), Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }
}