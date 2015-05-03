using BuildFeed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace BuildFeed.Controllers
{
    public class frontController : Controller
    {
        private const int _pageSize = 25;

        [Route("")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult index()
        {
            var buildGroups = from b in Build.Select()
                              group b by new BuildGroup()
                              {
                                  Major = b.MajorVersion,
                                  Minor = b.MinorVersion,
                                  Build = b.Number,
                                  Revision = b.Revision
                              } into bg
                              orderby bg.Key.Major descending,
                                      bg.Key.Minor descending,
                                      bg.Key.Build descending,
                                      bg.Key.Revision descending
                              select bg;
            return View(buildGroups);
        }

        [Route("group/{major}.{minor}.{number}.{revision}/")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewGroup(byte major, byte minor, ushort number, ushort? revision = null)
        {
            var builds = (from b in Build.Select()
                          group b by new BuildGroup()
                          {
                              Major = b.MajorVersion,
                              Minor = b.MinorVersion,
                              Build = b.Number,
                              Revision = b.Revision
                          } into bg
                          where bg.Key.Major == major
                          where bg.Key.Minor == minor
                          where bg.Key.Build == number
                          where bg.Key.Revision == revision
                          select bg).Single();

            return builds.Count() == 1 ?
                RedirectToAction("viewBuild", new { id = builds.Single().Id }) as ActionResult :
                View(builds);
        }

        [Route("build/{id}/")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewBuild(long id)
        {
            Build b = Build.SelectById(id);
            return View(b);
        }

        [Route("add/")]
        public ActionResult addBuild()
        {
            return View("editBuild");
        }

        [Route("edit/{id}/")]
        public ActionResult editBuild(long id)
        {
            Build b = Build.SelectById(id);
            return View(b);
        }
    }
}