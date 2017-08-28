using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using BuildFeed.Controllers;
using BuildFeed.Model;

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

        [Authorize(Roles = "Administrators")]
        [Route("regen-cache")]
        public async Task<ActionResult> RegenerateCache()
        {
            BuildRepository bRepo = new BuildRepository();
            await bRepo.RegenerateCachedProperties();

            return RedirectToAction(nameof(Index));
        }
    }
}