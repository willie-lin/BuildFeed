using BuildFeed.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BuildFeed.Controllers
{
    public class buildController : Controller
    {
        public static int pageSize { get { return 25; } }
        //
        // GET: /build/

        public ActionResult index(int page = 1)
        {
            return RedirectToActionPermanent("index", "front");
        }

        public ActionResult year(int year, int page = 1)
        {
            var builds = Build.SelectInBuildOrder().Where(b => b.BuildTime.HasValue && b.BuildTime.Value.Year == year);
            var pageBuilds = builds.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Count()) / Convert.ToDouble(pageSize));

            return View("index", pageBuilds);
        }

        public ActionResult lab(string lab, int page = 1)
        {
            return RedirectToActionPermanent("viewLab", "front", new { lab = lab });
        }

        public ActionResult version(int major, int minor, int page = 1)
        {
            var builds = Build.SelectInBuildOrder().Where(b => b.MajorVersion == major && b.MinorVersion == minor);
            var pageBuilds = builds.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Count()) / Convert.ToDouble(pageSize));

            return View("index", pageBuilds);
        }

        public ActionResult source(TypeOfSource source, int page = 1)
        {
            var builds = Build.SelectInBuildOrder().Where(b => b.SourceType == source);
            var pageBuilds = builds.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Count()) / Convert.ToDouble(pageSize));

            return View("index", pageBuilds);
        }

        //
        // GET: /build/Info/5

        public ActionResult info(int id)
        {
            return RedirectToActionPermanent("viewBuild", "front", new { id = id });
        }
    }
}
