using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BuildFeed.Code;
using BuildFeed.Local;
using BuildFeed.Model.View;
using MongoAuth;

namespace BuildFeed.Controllers
{
    public class AccountController : BaseController
    {
        [Route("login/")]
        public ActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("login/")]
        public ActionResult Login(LoginUser ru)
        {
            if (ModelState.IsValid)
            {
                bool isAuthenticated = Membership.ValidateUser(ru.UserName, ru.Password);

                if (isAuthenticated)
                {
                    int expiryLength = ru.RememberMe
                        ? 129600
                        : 60;

                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(ru.UserName, true, expiryLength);
                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    HttpCookie cookieTicket = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                    {
                        Expires = DateTime.Now.AddMinutes(expiryLength),
                        Path = FormsAuthentication.FormsCookiePath
                    };
                    Response.Cookies.Add(cookieTicket);

                    string returnUrl = string.IsNullOrEmpty(Request.QueryString["ReturnUrl"])
                        ? "/"
                        : Request.QueryString["ReturnUrl"];

                    return Redirect(returnUrl);
                }
            }

            ViewData["ErrorMessage"] = "The username and password are not valid.";
            return View(ru);
        }

        [Authorize]
        [Route("password/")]
        public ActionResult Password() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("password/")]
        public ActionResult Password(ChangePassword cp)
        {
            if (ModelState.IsValid)
            {
                MembershipUser user = Membership.GetUser();

                if (user != null)
                {
                    bool success = user.ChangePassword(cp.OldPassword, cp.NewPassword);

                    if (success)
                    {
                        return Redirect("/");
                    }
                }
            }

            ViewData["ErrorMessage"] = VariantTerms.Support_Error_ChangingPasswordFail;
            return View(cp);
        }

        [Route("logout/")]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        [Route("register/")]
        public ActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("register/")]
        public async Task<ActionResult> Register(RegistrationUser ru)
        {
            if (ModelState.IsValid)
            {
                MembershipUser mu = Membership.CreateUser(ru.UserName, ru.Password, ru.EmailAddress, "{IGNORE}", "{IGNORE}", false, out MembershipCreateStatus status);

                switch (status)
                {
                    case MembershipCreateStatus.Success:
                    {
                        Roles.AddUserToRole(mu.UserName, "Users");

                        MongoMembershipProvider provider = (MongoMembershipProvider)Membership.Provider;
                        Guid id = (Guid)mu.ProviderUserKey;
                        string hash = (await provider.GenerateValidationHash(id)).ToLower();

                        string fullUrl = Request.Url?.GetLeftPart(UriPartial.Authority) + Url.Action("Validate",
                            "Account",
                            new
                            {
                                id,
                                hash
                            });
                        await EmailManager.SendRegistrationEmail(mu, fullUrl);
                        return RedirectToAction(nameof(RegisterThanks));
                    }
                    case MembershipCreateStatus.InvalidPassword:
                        ViewData["ErrorMessage"] = VariantTerms.Support_Error_InvalidPassword;
                        break;
                    case MembershipCreateStatus.DuplicateEmail:
                        ViewData["ErrorMessage"] = VariantTerms.Support_Error_DuplicateEmail;
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        ViewData["ErrorMessage"] = VariantTerms.Support_Error_DuplicateUserName;
                        break;
                    default:
                        ViewData["ErrorMessage"] = VariantTerms.Support_Error_UnknownError;
                        break;
                }
            }

            return View(ru);
        }

        [Route("register/thanks/")]
        public ActionResult RegisterThanks() => View();

        [Route("validate/{id:guid}/{hash}/")]
        public async Task<ActionResult> Validate(Guid id, string hash)
        {
            MongoMembershipProvider provider = (MongoMembershipProvider)Membership.Provider;
            bool success = await provider.ValidateUserFromHash(id, hash);

            return View(success
                ? "validate-success"
                : "validate-failure");
        }
    }
}