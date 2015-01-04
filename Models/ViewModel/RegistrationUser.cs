using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BuildFeed.Models.ViewModel
{
    public class RegistrationUser
    {
        [Required]
        [DisplayName("Username")]
        public string UserName { get; set; }

        [Required]
        [MinLength(12)]
        [DisplayName("Enter password")]
        public string Password { get; set; }

        [Required]
        [MinLength(12)]
        [DisplayName("Confirm password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        [DisplayName("Email address")]
        public string EmailAddress { get; set; }
    }
}