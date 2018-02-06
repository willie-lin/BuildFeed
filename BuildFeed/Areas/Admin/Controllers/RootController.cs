using System.Threading.Tasks;
using System.Web.Mvc;
using BuildFeed.Controllers;
using BuildFeed.Model;

namespace BuildFeed.Admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    [RouteArea("admin")]
    [RoutePrefix("")]
    public class RootController : BaseController
    {
        [Route("")]
        public ActionResult Index() => View();

        [Route("regen-cache")]
        public async Task<ActionResult> RegenerateCache()
        {
            var bRepo = new BuildRepository();
            await bRepo.RegenerateCachedProperties();

            return RedirectToAction(nameof(Index));
        }
    }
}