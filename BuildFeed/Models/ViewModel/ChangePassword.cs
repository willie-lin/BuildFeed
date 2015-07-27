using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Models.ViewModel
{
    public class ChangePassword
    {
        [Required]
        [MinLength(8)]
        [Display(ResourceType = typeof(Support), Name = "EnterCurrentPassword")]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(8)]
        [Display(ResourceType = typeof(Support), Name = "EnterNewPassword")]
        public string NewPassword { get; set; }

        [Required]
        [MinLength(8)]
        [Display(ResourceType = typeof(Support), Name = "ConfirmNewPassword")]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}
