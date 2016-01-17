using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BuildFeed.Code;
using BuildFeed.Models;

namespace BuildFeed.Controllers
{
   public class FrontController : LocalController
   {
      public const int PageSize = 72;

      private readonly Build _bModel;
      private readonly MetaItem _mModel;

      public FrontController()
      {
         _bModel = new Build();
         _mModel = new MetaItem();
      }

      [Route("", Order = 1)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> Index() { return await IndexPage(1); }

      [Route("page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> IndexPage(int page)
      {
         var buildGroups = await _bModel.SelectAllGroups(PageSize, (page - 1) * PageSize);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(
                                          Convert.ToDouble(await _bModel.SelectAllGroupsCount()) /
                                          Convert.ToDouble(PageSize));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("Index", buildGroups);
      }

      [Route("group/{major}.{minor}.{number}.{revision}/")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> ViewGroup(uint major, uint minor, uint number, uint? revision = null)
      {
         BuildGroup bg = new BuildGroup
                         {
                            Major = major,
                            Minor = minor,
                            Build = number,
                            Revision = revision
                         };

         var builds = await _bModel.SelectGroup(bg);

         return builds.Count() == 1
                   ? RedirectToAction(nameof(ViewBuild), new
                                                         {
                                                            id = builds.Single()
                                                                       .Id
                                                         }) as ActionResult
                   : View(new Tuple<BuildGroup, List<BuildModel>>(bg, builds));
      }

      [Route("build/{id:guid}/", Name = "Build")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> ViewBuild(Guid id)
      {
         BuildModel b = await _bModel.SelectById(id);
         if (b == null)
         {
            return new HttpNotFoundResult();
         }
         return View(b);
      }

      [Route("build/{id:long}/", Name = "Build (Legacy)")]
      public async Task<ActionResult> ViewBuild(long id)
      {
         BuildModel b = await _bModel.SelectByLegacyId(id);
         if (b == null)
         {
            return new HttpNotFoundResult();
         }
         return RedirectToAction(nameof(ViewBuild), new
                                                    {
                                                       id = b.Id
                                                    });
      }

      [Route("twitter/{id:guid}/", Name = "Twitter")]
#if !DEBUG
      [OutputCache(Duration = 600, VaryByParam = "none")]
      [CustomContentType(ContentType = "image/png", Order = 2)]
#endif
      public async Task<ActionResult> TwitterCard(Guid id)
      {
         BuildModel b = await _bModel.SelectById(id);
         if (b == null)
         {
            return new HttpNotFoundResult();
         }

         string path = Path.Combine(Server.MapPath("~/content/card/"), $"{b.Family}.png");
         bool backExists = System.IO.File.Exists(path);

         using (Bitmap bm = backExists ? new Bitmap(path) : new Bitmap(1120, 600))
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

               if (!backExists)
               {
                  gr.FillRectangle(new SolidBrush(Color.FromArgb(0x27, 0x2b, 0x30)), 0, 0, 1120, 600);
               }

               gp.AddString("BUILDFEED", new FontFamily("Segoe UI"), (int) FontStyle.Bold, 32, new Point(40, 32),
                            StringFormat.GenericTypographic);
               gp.AddString($"{DisplayHelpers.GetDisplayTextForEnum(b.Family)} (WinNT {b.MajorVersion}.{b.MinorVersion})",
                            new FontFamily("Segoe UI"), 0, 48, new Point(40, 80), StringFormat.GenericTypographic);
               gp.AddString(b.Number.ToString(), new FontFamily("Segoe UI Light"), 0, 280, new Point(32, 96),
                            StringFormat.GenericTypographic);
               gp.AddString(b.BuildTime.HasValue ? $"{b.Lab}\r\n{b.BuildTime.Value:yyyy/MM/dd HH:mm}" : $"{b.Lab}",
                            new FontFamily("Segoe UI"), 0, 44, new Point(40, 440), StringFormat.GenericTypographic);
               gr.FillPath(Brushes.White, gp);

               Response.ContentType = "image/png";
               bm.Save(Response.OutputStream, ImageFormat.Png);
            }
         }

         return new EmptyResult();
      }

      [Route("twitter/{id:long}/", Name = "Twitter (Legacy)")]
      public async Task<ActionResult> TwitterCard(long id)
      {
         BuildModel b = await _bModel.SelectByLegacyId(id);
         if (b == null)
         {
            return new HttpNotFoundResult();
         }
         return RedirectToAction(nameof(TwitterCard), new
                                                      {
                                                         id = b.Id
                                                      });
      }

      [Route("lab/{lab}/", Order = 1, Name = "Lab Root")]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> ViewLab(string lab) { return await ViewLabPage(lab, 1); }

      [Route("lab/{lab}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> ViewLabPage(string lab, int page)
      {
         ViewBag.MetaItem = await _mModel.SelectById(new MetaItemKey
                                                     {
                                                        Type = MetaType.Lab,
                                                        Value = lab
                                                     });

         var builds = await _bModel.SelectLab(lab, PageSize, (page - 1) * PageSize);

         ViewBag.ItemId = builds.FirstOrDefault()
            ?.Lab;
         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(await _bModel.SelectLabCount(lab)) / Convert.ToDouble(PageSize));

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
      public async Task<ActionResult> ViewSource(TypeOfSource source) { return await ViewSourcePage(source, 1); }

      [Route("source/{source}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> ViewSourcePage(TypeOfSource source, int page)
      {
         ViewBag.MetaItem = await _mModel.SelectById(new MetaItemKey
                                                     {
                                                        Type = MetaType.Source,
                                                        Value = source.ToString()
                                                     });
         ViewBag.ItemId = DisplayHelpers.GetDisplayTextForEnum(source);

         var builds = await _bModel.SelectSource(source, PageSize, (page - 1) * PageSize);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(await _bModel.SelectSourceCount(source)) / Convert.ToDouble(PageSize));

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
      public async Task<ActionResult> ViewYear(int year) { return await ViewYearPage(year, 1); }

      [Route("year/{year}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "page", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> ViewYearPage(int year, int page)
      {
         ViewBag.MetaItem = await _mModel.SelectById(new MetaItemKey
                                                     {
                                                        Type = MetaType.Year,
                                                        Value = year.ToString()
                                                     });
         ViewBag.ItemId = year.ToString();

         var builds = await _bModel.SelectYear(year, PageSize, (page - 1) * PageSize);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(await _bModel.SelectYearCount(year) / Convert.ToDouble(PageSize));

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
      public async Task<ActionResult> ViewVersion(uint major, uint minor) { return await ViewVersionPage(major, minor, 1); }

      [Route("version/{major}.{minor}/page-{page:int:min(2)}/", Order = 0)]
#if !DEBUG
//      [OutputCache(Duration = 600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
      public async Task<ActionResult> ViewVersionPage(uint major, uint minor, int page)
      {
         string valueString = $"{major}.{minor}";
         ViewBag.MetaItem = await _mModel.SelectById(new MetaItemKey
                                                     {
                                                        Type = MetaType.Version,
                                                        Value = valueString
                                                     });
         ViewBag.ItemId = valueString;

         var builds = await _bModel.SelectVersion(major, minor, PageSize, (page - 1) * PageSize);

         ViewBag.PageNumber = page;
         ViewBag.PageCount = Math.Ceiling(Convert.ToDouble(await _bModel.SelectVersionCount(major, minor)) / Convert.ToDouble(PageSize));

         if (ViewBag.PageNumber > ViewBag.PageCount)
         {
            return new HttpNotFoundResult();
         }

         return View("viewVersion", builds);
      }

      [Route("add/"), Authorize]
      public ActionResult AddBuild()
      {
         BuildModel b = new BuildModel
                        {
                           SourceType = TypeOfSource.PrivateLeak,
                           FlightLevel = LevelOfFlight.None
                        };
         return View("EditBuild", b);
      }

      [Route("add/"), Authorize, HttpPost]
      public async Task<ActionResult> AddBuild(BuildModel build)
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
               await _bModel.Insert(build);
            }
            catch
            {
               return View("EditBuild", build);
            }
            return RedirectToAction(nameof(ViewBuild), new
                                                       {
                                                          id = build.Id
                                                       });
         }
         return View("EditBuild", build);
      }

      [Route("edit/{id}/"), Authorize]
      public async Task<ActionResult> EditBuild(Guid id)
      {
         BuildModel b = await _bModel.SelectById(id);
         return View(b);
      }

      [Route("edit/{id}/"), Authorize, HttpPost]
      public async Task<ActionResult> EditBuild(Guid id, BuildModel build)
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
               await _bModel.Update(build);
            }
            catch
            {
               return View(build);
            }

            return RedirectToAction(nameof(ViewBuild), new
                                                       {
                                                          id = build.Id
                                                       });
         }
         return View(build);
      }

      [Route("delete/{id}/"), Authorize(Roles = "Administrators")]
      public async Task<ActionResult> DeleteBuild(Guid id)
      {
         await _bModel.DeleteById(id);
         return RedirectToAction(nameof(Index));
      }
   }
}