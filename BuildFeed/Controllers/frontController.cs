using BuildFeed.Models;
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

      private Build bModel;

      public frontController() : base()
      {
         bModel = new Build();
      }

      [Route("", Order = 1)]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult index() { return indexPage(1); }

      [Route("page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public ActionResult indexPage(int page)
      {
         var buildGroups = bModel.SelectBuildGroups(PAGE_SIZE, (page - 1) * PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(
            Convert.ToDouble(bModel.SelectBuildGroupsCount()) /
            Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("index", buildGroups);
      }

      [Route("group/{major}.{minor}.{number}.{revision}/")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult viewGroup(byte major, byte minor, ushort number, ushort? revision = null)
      {
         var builds = bModel.SelectSingleBuildGroup(new BuildGroup()
         {
            Major = major,
            Minor = minor,
            Build = number,
            Revision = revision
         });

         return builds.Item2.Count() == 1 ?
                    RedirectToAction("viewBuild", new { id = builds.Item2.Single().Id }) as ActionResult :
                    View(builds);
      }

      [Route("build/{id}/", Name = "Build")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult viewBuild(Guid id)
      {
         BuildModel b = bModel.SelectById(id);
         return View(b);
      }

      [Route("twitter/{id}/")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none")]
      [CustomContentType(ContentType = "image/png", Order = 2)]
#endif
      public ActionResult twitterCard(Guid id)
      {
         BuildModel b = bModel.SelectById(id);

         using (Bitmap bm = new Bitmap(560, 300))
         {
            using (Graphics gr = Graphics.FromImage(bm))
            {
               GraphicsPath gp = new GraphicsPath();
               gr.CompositingMode = CompositingMode.SourceOver;
               gr.CompositingQuality = CompositingQuality.HighQuality;
               gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
               gr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
               gr.SmoothingMode = SmoothingMode.HighQuality;
               gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

               gr.FillRectangle(new SolidBrush(Color.FromArgb(0x27, 0x2b, 0x30)), 0, 0, 560, 300);
               gp.AddString("BUILDFEED", new FontFamily("Segoe UI"), (int)FontStyle.Bold, 16, new Point(20, 20), StringFormat.GenericTypographic);
               gp.AddString($"Windows NT {b.MajorVersion}.{b.MinorVersion} build", new FontFamily("Segoe UI"), 0, 24, new Point(20, 40), StringFormat.GenericTypographic);
               gp.AddString(b.Number.ToString(), new FontFamily("Segoe UI Light"), 0, 180, new Point(12, 20), StringFormat.GenericTypographic);
               gp.AddString($"{b.Lab}", new FontFamily("Segoe UI"), 0, 40, new Point(16, 220), StringFormat.GenericTypographic);
               gr.FillPath(Brushes.White, gp);

               Response.ContentType = "image/png";
               bm.Save(Response.OutputStream, ImageFormat.Png);
            }
         }

         return new EmptyResult();
      }

      [Route("lab/{lab}/", Order = 1, Name = "Lab Root")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult viewLab(string lab) { return viewLabPage(lab, 1); }

      [Route("lab/{lab}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public ActionResult viewLabPage(string lab, int page)
      {
         ViewBag.MetaItem = new MetaItem().SelectById(new MetaItemKey
         {
            Type = MetaType.Lab,
            Value = lab
         });
         ViewBag.ItemId = lab;

         var builds = bModel.SelectLab(lab, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(bModel.SelectLabCount(lab)) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewLab", builds);
      }

      [Route("source/{source}/", Order = 1, Name = "Source Root")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult viewSource(TypeOfSource source) { return viewSourcePage(source, 1); }

      [Route("source/{source}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public ActionResult viewSourcePage(TypeOfSource source, int page)
      {
         ViewBag.MetaItem = new MetaItem().SelectById(new MetaItemKey
         {
            Type = MetaType.Source,
            Value = source.ToString()
         });
         ViewBag.ItemId = DisplayHelpers.GetDisplayTextForEnum(source);

         var builds = bModel.SelectSource(source, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(bModel.SelectSourceCount(source)) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewSource", builds);
      }

      [Route("year/{year}/", Order = 1, Name = "Year Root")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult viewYear(int year) { return viewYearPage(year, 1); }

      [Route("year/{year}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public ActionResult viewYearPage(int year, int page)
      {
         ViewBag.MetaItem = new MetaItem().SelectById(new MetaItemKey
         {
            Type = MetaType.Year,
            Value = year.ToString()
         });
         ViewBag.ItemId = year.ToString();

         var builds = bModel.SelectYear(year, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(bModel.SelectYearCount(year) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewYear", builds);
      }

      [Route("version/{major}.{minor}/", Order = 1, Name = "Version Root")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult viewVersion(int major, int minor) { return viewVersionPage(major, minor, 1); }

      [Route("version/{major}.{minor}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public ActionResult viewVersionPage(int major, int minor, int page)
      {
         string valueString = $"{major}.{minor}";
         ViewBag.MetaItem = new MetaItem().SelectById(new MetaItemKey
         {
            Type = MetaType.Version,
            Value = valueString
         });
         ViewBag.ItemId = valueString;

         var builds = bModel.SelectVersion(major, minor, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(bModel.SelectVersionCount(major, minor)) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewVersion", builds);
      }

      [Route("add/"), Authorize]
      public ActionResult addBuild() { return View("editBuild"); }

      [Route("add/"), Authorize, HttpPost]
      public ActionResult addBuild(BuildModel build)
      {
         if (ModelState.IsValid)
         {
            try
            {
               build.Added = DateTime.Now;
               build.Modified = DateTime.Now;
               new Build().Insert(build);
            }
            catch
            {
               return View("editBuild", build);
            }
            return RedirectToAction("viewBuild", new
            {
               id = build.Id
            });
         }
         return View("editBuild", build);
      }

      [Route("edit/{id}/"), Authorize]
      public ActionResult editBuild(Guid id)
      {
         BuildModel b = new Build().SelectById(id);
         return View(b);
      }

      [Route("edit/{id}/"), Authorize, HttpPost]
      public ActionResult editBuild(long id, BuildModel build)
      {
         if (ModelState.IsValid)
         {
            try
            {
               new Build().Update(build);
            }
            catch
            {
               return View(build);
            }

            return RedirectToAction("viewBuild", new { id = build.Id });
         }
         return View(build);
      }

      [Route("delete/{id}/"), Authorize(Roles = "Administrators")]
      public ActionResult deleteBuild(Guid id)
      {
         new Build().DeleteById(id);
         return RedirectToAction("index");
      }
   }
}