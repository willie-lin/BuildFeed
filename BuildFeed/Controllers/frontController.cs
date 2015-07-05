using BuildFeed.Code;
using BuildFeed.Models;
using BuildFeed.Models.ViewModel.Front;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Web.Mvc;

namespace BuildFeed.Controllers
{
    public class frontController : Controller
    {
        public const int PAGE_SIZE = 96;

        [Route("", Order = 1)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult index()
        {
            return indexPage(1);
        }

        [Route("page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
        public ActionResult indexPage(int page)
        {
            var buildGroups = (from b in Build.Select()
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
                              }).ToArray();

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(buildGroups.Length) / Convert.ToDouble(PAGE_SIZE));

            if (ViewBag.PageNumber > ViewBag.PageCount)
            {
                return new HttpNotFoundResult();
            }

            return View("index", buildGroups.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE));
        }

        [Route("group/{major}.{minor}.{number}.{revision}/")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
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

        [Route("build/{id}/", Name = "Build")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult viewBuild(long id)
        {
            Build b = Build.SelectById(id);
            return View(b);
        }

        [Route("twitter/{id}/")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none")]
        [CustomContentType(ContentType = "image/png", Order = 2)]
#endif
        public ActionResult twitterCard(long id)
        {
            Build b = Build.SelectById(id);

            using (Bitmap bm = new Bitmap(560, 300))
            using (Graphics gr = Graphics.FromImage(bm))
            {
                GraphicsPath gp = new GraphicsPath();
                gr.CompositingMode = CompositingMode.SourceOver;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                gr.FillRectangle(new SolidBrush(Color.FromArgb(0x30, 0x30, 0x30)), 0, 0, 560, 300);
                gp.AddString("BUILDFEED", new FontFamily("Segoe UI"), (int)FontStyle.Bold, 16, new Point(20, 20), StringFormat.GenericTypographic);
                gp.AddString($"Windows NT {b.MajorVersion}.{b.MinorVersion} build", new FontFamily("Segoe UI"), 0, 24, new Point(20, 40), StringFormat.GenericTypographic);
                gp.AddString(b.Number.ToString(), new FontFamily("Segoe UI Light"), 0, 180, new Point(12, 20), StringFormat.GenericTypographic);
                gp.AddString($"{b.Lab}", new FontFamily("Segoe UI"), 0, 40, new Point(16, 220), StringFormat.GenericTypographic);
                gr.FillPath(Brushes.White, gp);

                Response.ContentType = "image/png";
                bm.Save(Response.OutputStream, ImageFormat.Png);
            }

            return new EmptyResult();
        }

        [Route("lab/{lab}/", Order = 1, Name = "Lab Root")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult viewLab(string lab)
        {
            return viewLabPage(lab, 1);
        }

        [Route("lab/{lab}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
        public ActionResult viewLabPage(string lab, int page)
        {
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Lab, Value = lab });
            ViewBag.ItemId = lab;

            var builds = Build.SelectInBuildOrder().Where(b => b.Lab != null && (b.Lab.ToLower() == lab.ToLower())).ToArray();

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Length) / Convert.ToDouble(PAGE_SIZE));

            if (ViewBag.PageNumber > ViewBag.PageCount)
            {
                return new HttpNotFoundResult();
            }

            return View("viewLab", builds.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE));
        }

        [Route("source/{source}/", Order = 1, Name = "Source Root")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult viewSource(TypeOfSource source)
        {
            return viewSourcePage(source, 1);
        }

        [Route("source/{source}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
        public ActionResult viewSourcePage(TypeOfSource source, int page)
        {
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Source, Value = source.ToString() });
            ViewBag.ItemId = DisplayHelpers.GetDisplayTextForEnum(source);

            var builds = Build.SelectInBuildOrder().Where(b => b.SourceType == source).ToArray();

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Length) / Convert.ToDouble(PAGE_SIZE));

            if (ViewBag.PageNumber > ViewBag.PageCount)
            {
                return new HttpNotFoundResult();
            }

            return View("viewSource", builds.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE));
        }

        [Route("year/{year}/", Order = 1, Name = "Year Root")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult viewYear(int year)
        {
            return viewYearPage(year, 1);
        }

        [Route("year/{year}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
        public ActionResult viewYearPage(int year, int page)
        {
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Year, Value = year.ToString() });
            ViewBag.ItemId = year.ToString();

            var builds = Build.SelectInBuildOrder().Where(b => b.BuildTime.HasValue && b.BuildTime.Value.Year == year).ToArray();

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Length) / Convert.ToDouble(PAGE_SIZE));

            if (ViewBag.PageNumber > ViewBag.PageCount)
            {
                return new HttpNotFoundResult();
            }

            return View("viewYear", builds.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE));
        }

        [Route("version/{major}.{minor}/", Order = 1, Name = "Version Root")]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult viewVersion(int major, int minor)
        {
            return viewVersionPage(major, minor, 1);
        }

        [Route("version/{major}.{minor}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
        [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult viewVersionPage(int major, int minor, int page)
        {
            string valueString = $"{major}.{minor}";
            ViewBag.MetaItem = MetaItem.SelectById(new MetaItemKey() { Type = MetaType.Version, Value = valueString });
            ViewBag.ItemId = valueString;

            var builds = Build.SelectInBuildOrder().Where(b => b.MajorVersion == major && b.MinorVersion == minor).ToArray();

            ViewBag.PageNumber = page;
            ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(builds.Length) / Convert.ToDouble(PAGE_SIZE));

            if (ViewBag.PageNumber > ViewBag.PageCount)
            {
                return new HttpNotFoundResult();
            }

            return View("viewVersion", builds.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE));
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

        [Route("delete/{id}/"), Authorize(Roles = "Administrators")]
        public ActionResult deleteBuild(long id)
        {
            Build.DeleteById(id);
            return RedirectToAction("index");
        }
    }
}