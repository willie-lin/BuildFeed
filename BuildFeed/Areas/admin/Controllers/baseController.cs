using System.Web.Mvc;
using System.Web.Security;

namespace BuildFeed.Areas.admin.Controllers
{
    public class baseController : Controller
    {
        [Authorize(Roles = "Administrators")]
        // GET: admin/base
        public ActionResult index() { return View(); }

        [Authorize(Users = "hounsell")]
        public ActionResult setup()
        {
            if (!Roles.RoleExists("Administrators"))
            {
                Roles.CreateRole("Administrators");
            }
            if (!Roles.IsUserInRole("hounsell", "Administrators"))
            {
                Roles.AddUserToRole("hounsell", "Administrators");
            }

            return RedirectToAction("index");
        }
    }
}