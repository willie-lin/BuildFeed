using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BuildFeed.Models.ViewModel
{
    public class QuestionForm
    {
        [Required]
        [DisplayName("Your name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Your email address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DisplayName("Question")]
        public string Comment { get; set; }
    }
}