using System.Web.Mvc;
using System.Web.Security;
using BuildFeed.Controllers;

namespace BuildFeed.Admin.Controllers
{
    [RouteArea("admin")]
    [RoutePrefix("")]
    public class RootController : BaseController
    {
        [Authorize(Roles = "Administrators")]
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Users = "hounsell")]
        [Route("setup")]
        public ActionResult Setup()
        {
            if (!Roles.RoleExists("Administrators"))
            {
                Roles.CreateRole("Administrators");
            }
            if (!Roles.IsUserInRole("hounsell", "Administrators"))
            {
                Roles.AddUserToRole("hounsell", "Administrators");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}