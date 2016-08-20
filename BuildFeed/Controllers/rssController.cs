using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BuildFeed.Model;
using WilderMinds.RssSyndication;

namespace BuildFeed.Controllers
{
   public class RssController : BaseController
   {
      private const int RSS_SIZE = 25;
      private readonly BuildRepository _bModel;

      public RssController() { _bModel = new BuildRepository(); }

      [Route("rss/compiled")]
      public async Task<ActionResult> Index()
      {
         List<Build> builds = await _bModel.SelectBuildsByCompileDate(RSS_SIZE);

         Feed feed = new Feed
         {
            Title = "BuildFeed RSS - Recently Compiled",
            Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
            Items = (from build in builds
                     select new Item
                     {
                        Title = build.FullBuildString,
                        Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = {
                           build.Family.ToString()
                        },
                        PublishDate = DateTime.SpecifyKind(build.BuildTime.GetValueOrDefault(), DateTimeKind.Utc)
                     }).ToList()
         };

         return new ContentResult
         {
            Content = feed.Serialize(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/added")]
      public async Task<ActionResult> Added()
      {
         List<Build> builds = await _bModel.SelectBuildsByAddedDate(RSS_SIZE);

         Feed feed = new Feed
         {
            Title = "BuildFeed RSS - Recently Added",
            Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
            Items = (from build in builds
                     select new Item
                     {
                        Title = build.FullBuildString,
                        Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = {
                           build.Family.ToString()
                        },
                        PublishDate = DateTime.SpecifyKind(build.Added, DateTimeKind.Utc)
                     }).ToList()
         };

         return new ContentResult
         {
            Content = feed.Serialize(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/leaked")]
      public async Task<ActionResult> Leaked()
      {
         List<Build> builds = await _bModel.SelectBuildsByLeakedDate(RSS_SIZE);

         Feed feed = new Feed
         {
            Title = "BuildFeed RSS - Recently Leaked",
            Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
            Items = (from build in builds
                     select new Item
                     {
                        Title = build.FullBuildString,
                        Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = {
                           build.Family.ToString()
                        },
                        PublishDate = DateTime.SpecifyKind(build.LeakDate.GetValueOrDefault(), DateTimeKind.Utc)
                     }).ToList()
         };

         return new ContentResult
         {
            Content = feed.Serialize(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/version")]
      public async Task<ActionResult> Version()
      {
         List<Build> builds = await _bModel.SelectBuildsByOrder(RSS_SIZE);

         Feed feed = new Feed
         {
            Title = "BuildFeed RSS - Highest Version",
            Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
            Items = (from build in builds
                     select new Item
                     {
                        Title = build.FullBuildString,
                        Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = {
                           build.Family.ToString()
                        }
                     }).ToList()
         };

         return new ContentResult
         {
            Content = feed.Serialize(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/lab/{lab}")]
      public async Task<ActionResult> Lab(string lab)
      {
         List<Build> builds = await _bModel.SelectLab(lab, RSS_SIZE);

         Feed feed = new Feed
         {
            Title = $"BuildFeed RSS - {lab} Lab",
            Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
            Items = (from build in builds
                     select new Item
                     {
                        Title = build.FullBuildString,
                        Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = {
                           build.Family.ToString()
                        }
                     }).ToList()
         };

         return new ContentResult
         {
            Content = feed.Serialize(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }
   }
}