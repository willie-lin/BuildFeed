using BuildFeed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BuildFeed.Controllers
{
    public class frontController : Controller
    {
        private const int _pageSize = 25;

        [Route("")]
        [OutputCache(Duration = 600, VaryByParam = "none")]
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
    }
}