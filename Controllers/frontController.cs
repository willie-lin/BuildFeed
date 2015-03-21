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

        [Route("group/{major}.{minor}.{number}.{revision}")]
        public ActionResult viewGroup(byte major, byte minor, ushort number, ushort? revision = null)
        {
            var builds = from b in Build.Select()
                         where b.MajorVersion == major
                         where b.MinorVersion == minor
                         where b.Number == number
                         where b.Revision == revision
                         orderby b.BuildTime descending
                         select b;

            return PartialView(builds);
        }
    }
}