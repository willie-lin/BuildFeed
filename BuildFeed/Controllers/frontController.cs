using BuildFeed.Code;
using BuildFeed.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace BuildFeed.Controllers
{
   public class frontController : LocalController
   {
      public const int PAGE_SIZE = 96;

      private Build bModel;
      private MetaItem mModel;

      public frontController() : base()
      {
         bModel = new Build();
         mModel = new MetaItem();
      }

      [Route("", Order = 1)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> index() { return await indexPage(1); }

      [Route("page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> indexPage(int page)
      {
         var buildGroups = await bModel.SelectBuildGroups(PAGE_SIZE, (page - 1) * PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(
            Convert.ToDouble(await bModel.SelectBuildGroupsCount()) /
            Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("index", buildGroups);
      }

      [Route("group/{major}.{minor}.{number}.{revision}/")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewGroup(byte major, byte minor, ushort number, ushort? revision = null)
      {
         var builds = await bModel.SelectSingleBuildGroup(new BuildGroup()
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

      [Route("build/{id:guid}/", Name = "Build")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewBuild(Guid id)
      {
         BuildModel b = await bModel.SelectById(id);
         return View(b);
      }

      [Route("build/{id:long}/", Name = "Build (Legacy)")]
      public async Task<ActionResult> viewBuild(long id)
      {
         BuildModel b = await bModel.SelectByLegacyId(id);
         return RedirectToAction("viewBuild", new { id = b.Id });
      }

      [Route("twitter/{id}/")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none")]
      [CustomContentType(ContentType = "image/png", Order = 2)]
#endif
      public async Task<ActionResult> twitterCard(Guid id)
      {
         BuildModel b = await bModel.SelectById(id);

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
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewLab(string lab) { return await viewLabPage(lab, 1); }

      [Route("lab/{lab}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewLabPage(string lab, int page)
      {
         ViewBag.MetaItem = await mModel.SelectById(new MetaItemKey
         {
            Type = MetaType.Lab,
            Value = lab
         });

         var builds = await bModel.SelectLab(lab, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.ItemId = builds.First().Lab;
         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(await bModel.SelectLabCount(lab)) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewLab", builds);
      }

      [Route("source/{source}/", Order = 1, Name = "Source Root")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewSource(TypeOfSource source) { return await viewSourcePage(source, 1); }

      [Route("source/{source}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewSourcePage(TypeOfSource source, int page)
      {
         ViewBag.MetaItem = await mModel.SelectById(new MetaItemKey
         {
            Type = MetaType.Source,
            Value = source.ToString()
         });
         ViewBag.ItemId = DisplayHelpers.GetDisplayTextForEnum(source);

         var builds = await bModel.SelectSource(source, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(await bModel.SelectSourceCount(source)) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewSource", builds);
      }

      [Route("year/{year}/", Order = 1, Name = "Year Root")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewYear(int year) { return await viewYearPage(year, 1); }

      [Route("year/{year}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewYearPage(int year, int page)
      {
         ViewBag.MetaItem = await mModel.SelectById(new MetaItemKey
         {
            Type = MetaType.Year,
            Value = year.ToString()
         });
         ViewBag.ItemId = year.ToString();

         var builds = await bModel.SelectYear(year, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(await bModel.SelectYearCount(year) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewYear", builds);
      }

      [Route("version/{major}.{minor}/", Order = 1, Name = "Version Root")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewVersion(int major, int minor) { return await viewVersionPage(major, minor, 1); }

      [Route("version/{major}.{minor}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> viewVersionPage(int major, int minor, int page)
      {
         string valueString = $"{major}.{minor}";
         ViewBag.MetaItem = await mModel.SelectById(new MetaItemKey
         {
            Type = MetaType.Version,
            Value = valueString
         });
         ViewBag.ItemId = valueString;

         var builds = await bModel.SelectVersion(major, minor, (page - 1) * PAGE_SIZE, PAGE_SIZE);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(await bModel.SelectVersionCount(major, minor)) / Convert.ToDouble(PAGE_SIZE));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewVersion", builds);
      }

      [Route("add/"), Authorize]
      public ActionResult addBuild()
      {
         BuildModel b = new BuildModel()
         {
            SourceType = TypeOfSource.PrivateLeak,
            FlightLevel = LevelOfFlight.None
         };
         return View("editBuild", b);
      }

      [Route("add/"), Authorize, HttpPost]
      public async Task<ActionResult> addBuild(BuildModel build)
      {
         if (ModelState.IsValid)
         {
            try
            {
               build.Added = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
               build.Modified = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
               if (build.BuildTime.HasValue)
               {
                  build.BuildTime = DateTime.SpecifyKind(build.BuildTime.Value, DateTimeKind.Utc);
               }
               if (build.LeakDate.HasValue)
               {
                  build.LeakDate = DateTime.SpecifyKind(build.LeakDate.Value, DateTimeKind.Utc);
               }
               await bModel.Insert(build);
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
      public async Task<ActionResult> editBuild(Guid id)
      {
         BuildModel b = await bModel.SelectById(id);
         return View(b);
      }

      [Route("edit/{id}/"), Authorize, HttpPost]
      public async Task<ActionResult> editBuild(Guid id, BuildModel build)
      {
         if (ModelState.IsValid)
         {
            try
            {
               if (build.BuildTime.HasValue)
               {
                  build.BuildTime = DateTime.SpecifyKind(build.BuildTime.Value, DateTimeKind.Utc);
               }
               if (build.LeakDate.HasValue)
               {
                  build.LeakDate = DateTime.SpecifyKind(build.LeakDate.Value, DateTimeKind.Utc);
               }
               await bModel.Update(build);
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
      public async Task<ActionResult> deleteBuild(Guid id)
      {
         await bModel.DeleteById(id);
         return RedirectToAction("index");
      }
   }
}