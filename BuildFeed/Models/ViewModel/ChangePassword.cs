using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BuildFeed.Models.ViewModel
{
    public class ChangePassword
    {
        [Required]
        [MinLength(8)]
        [DisplayName("Enter current password")]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(8)]
        [DisplayName("Enter new password")]
        public string NewPassword { get; set; }

        [Required]
        [MinLength(8)]
        [DisplayName("Confirm new password")]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}
