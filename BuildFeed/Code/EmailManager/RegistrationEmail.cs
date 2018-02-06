using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Security;
using BuildFeed.Local;

namespace BuildFeed.Code
{
    public static partial class EmailManager
    {
        public static async Task SendRegistrationEmail(MembershipUser mu, string validationLink)
        {
            using (var mm = new MailMessage(EMAIL_FROM, mu.Email))
            {
                mm.Subject = string.Format(VariantTerms.Email_Registration_Subject, InvariantTerms.SiteName);
                mm.Body = string.Format(VariantTerms.Email_Registration_Body, InvariantTerms.SiteName, validationLink);

                using (var sc = new SmtpClient())
                {
                    await sc.SendMailAsync(mm);
                }
            }
        }
    }
}