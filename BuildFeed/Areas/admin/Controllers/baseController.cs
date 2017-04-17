using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using BuildFeed.Controllers;
using BuildFeed.Model;

namespace BuildFeed.Areas.admin.Controllers
{
    public class baseController : BaseController
    {
        [Authorize(Roles = "Administrators")]
        // GET: admin/base
        public ActionResult index()
        {
            return View();
        }

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

        [Authorize(Users = "hounsell")]
        public ActionResult exception()
        {
            throw new Exception("This is a test exception");
        }

        [Authorize(Users = "hounsell")]
        public async Task<ActionResult> migrate()
        {
            BuildRepository _bModel = new BuildRepository();
            await _bModel.MigrateAddedModifiedToHistory();

            return RedirectToAction("index");
        }

        [Authorize(Users = "hounsell")]
        public async Task<ActionResult> cache()
        {
            BuildRepository _bModel = new BuildRepository();
            await _bModel.RegenerateCachedProperties();

            return RedirectToAction("index");
        }
    }
}