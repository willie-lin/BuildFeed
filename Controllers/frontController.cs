using BuildFeed.Models;
using BuildFeed.Models.ViewModel.Front;
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
        public const int _pageSize = 96;

        [Route("", Order = 1)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult index()
        {
            return indexPage(1);
        }

        [Route("page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult indexPage(int page)
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
                              select new FrontBuildGroup()
                              {
                                  Key = bg.Key,
                                  LastBuild = bg.Max(m => m.BuildTime),
                                  BuildCount = bg.Count()
                              };

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(buildGroups.Count()) / Convert.ToDouble(_pageSize));

            return View("index", buildGroups.Skip((page - 1) * _pageSize).Take(_pageSize));
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

        [Route("lab/{lab}/")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewLab(string lab)
        {
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Lab, Value = lab });
            ViewBag.ItemId = lab;

            var builds = Build.SelectInBuildOrder().Where(b => b.Lab != null && (b.Lab.ToLower() == lab.ToLower()));
            return View(builds);
        }

        [Route("source/{source}/", Order = 1)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewSource(TypeOfSource source)
        {
            return viewSourcePage(source, 1);
        }

        [Route("source/{source}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewSourcePage(TypeOfSource source, int page)
        {
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Source, Value = source.ToString() });
            ViewBag.ItemId = DisplayHelpers.GetDisplayTextForEnum(source);

            var builds = Build.SelectInBuildOrder().Where(b => b.SourceType == source);

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Count()) / Convert.ToDouble(_pageSize));

            return View("viewSource", builds.Skip((page - 1) * _pageSize).Take(_pageSize));
        }

        [Route("year/{year}/", Order = 1)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewYear(int year)
        {
            return viewYearPage(year, 1);
        }

        [Route("year/{year}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewYearPage(int year, int page)
        {
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Year, Value = year.ToString() });
            ViewBag.ItemId = year.ToString();

            var builds = Build.SelectInBuildOrder().Where(b => b.BuildTime.HasValue && b.BuildTime.Value.Year == year);

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Count()) / Convert.ToDouble(_pageSize));

            return View("viewYear", builds.Skip((page - 1) * _pageSize).Take(_pageSize));
        }

        [Route("version/{major}.{minor}/")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
#endif
        public ActionResult viewVersion(int major, int minor)
        {
            string valueString = string.Format("{0}.{1}", major, minor);
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Version, Value = valueString });
            ViewBag.ItemId = valueString;

            var builds = Build.SelectInBuildOrder().Where(b => b.MajorVersion == major && b.MinorVersion == minor);
            return View(builds);
        }

        [Route("add/"), Authorize]
        public ActionResult addBuild()
        {
            return View("editBuild");
        }

        [Route("add/"), Authorize, HttpPost]
        public ActionResult addBuild(Build build)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    build.Added = DateTime.Now;
                    build.Modified = DateTime.Now;
                    Build.Insert(build);
                }
                catch
                {
                    return View("editBuild", build);
                }
                return RedirectToAction("viewBuild", new { id = build.Id });
            }
            else
            {
                return View("editBuild", build);
            }
        }

        [Route("edit/{id}/"), Authorize]
        public ActionResult editBuild(long id)
        {
            Build b = Build.SelectById(id);
            return View(b);
        }

        [Route("edit/{id}/"), Authorize, HttpPost]
        public ActionResult editBuild(long id, Build build)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Build.Update(build);
                }
                catch
                {
                    return View(build);
                }

                return RedirectToAction("viewBuild", new { id = build.Id });
            }
            else
            {
                return View(build);
            }
        }

        [Route("delete/{id}/"), Authorize(Roles = "Adminstrators")]
        public ActionResult deleteBuild(long id)
        {
            Build.DeleteById(id);
            return RedirectToAction("index");
        }
    }
}