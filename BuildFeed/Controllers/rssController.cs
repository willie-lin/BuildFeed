using BuildFeed.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using X.Web.RSS;
using X.Web.RSS.Enumerators;
using X.Web.RSS.Structure;
using X.Web.RSS.Structure.Validators;

namespace BuildFeed.Controllers
{
   public class rssController : Controller
   {
      private Build bModel;
      private const int RSS_SIZE = 20;

      public rssController() : base()
      {
         bModel = new Build();
      }

      [Route("rss/compiled")]
      public async Task<ActionResult> index()
      {
         var builds = await bModel.SelectInBuildOrder(RSS_SIZE, 0);

         RssDocument rdoc = new RssDocument()
         {
            Channel = new RssChannel()
            {
               Title = "BuildFeed RSS - Recently Compiled",
               Description = "",
               Generator = "BuildFeed.net RSS Controller",
               Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}"),
               SkipHours = new List<Hour>(),
               SkipDays = new List<Day>(),

               Items = (from build in builds
                        select new RssItem()
                        {
                           Title = build.FullBuildString,
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"),
                           Guid = new RssGuid() { IsPermaLink = true, Value = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("viewBuild", new { controller = "front", id = build.Id })) },
                        }).ToList()
            }
         };

         Response.ContentType = "application/rss+xml";

         await Response.Output.WriteAsync(rdoc.ToXml());

         return new EmptyResult();
      }

      [Route("rss/added")]
      public async Task<ActionResult> added()
      {
         var builds = await bModel.SelectLatest(RSS_SIZE, 0);

         RssDocument rdoc = new RssDocument()
         {
            Channel = new RssChannel()
            {
               Title = "BuildFeed RSS - Recently Added",
               Description = "",
               Generator = "BuildFeed.net RSS Controller",
               Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}"),
               SkipHours = new List<Hour>(),
               SkipDays = new List<Day>(),

               Items = (from build in builds
                        select new RssItem()
                        {
                           Title = build.FullBuildString,
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"
                           },
                           InternalPubDate = new RssDate(build.Added).DateStringISO8601 // bit of a dirty hack to work around problem in X.Web.RSS with the date format.
                        }).ToList()
            }
         };

         Response.ContentType = "application/rss+xml";

         await Response.Output.WriteAsync(rdoc.ToXml());

         return new EmptyResult();
      }

      [Route("rss/leaked")]
      public async Task<ActionResult> leaked()
      {
         var builds = await bModel.SelectLatestLeaked(RSS_SIZE, 0);

         RssDocument rdoc = new RssDocument()
         {
            Channel = new RssChannel()
            {
               Title = "BuildFeed RSS - Recently Leaked",
               Description = "",
               Generator = "BuildFeed.net RSS Controller",
               Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}"),
               SkipHours = new List<Hour>(),
               SkipDays = new List<Day>(),

               Items = (from build in builds
                        select new RssItem()
                        {
                           Title = build.FullBuildString,
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"
                           },
                           InternalPubDate = new RssDate(build.LeakDate.Value).DateStringISO8601 // bit of a dirty hack to work around problem in X.Web.RSS with the date format.
                        }).ToList()
            }
         };

         Response.ContentType = "application/rss+xml";

         await Response.Output.WriteAsync(rdoc.ToXml());

         return new EmptyResult();
      }

      [Route("rss/version")]
      public async Task<ActionResult> version()
      {
         var builds = await bModel.SelectInVersionOrder(RSS_SIZE, 0);


         RssDocument rdoc = new RssDocument()
         {
            Channel = new RssChannel()
            {
               Title = "BuildFeed RSS - Highest Version",
               Description = "",
               Generator = "BuildFeed.net RSS Controller",
               Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}"),
               SkipHours = new List<Hour>(),
               SkipDays = new List<Day>(),

               Items = (from build in builds
                        select new RssItem()
                        {
                           Title = build.FullBuildString,
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"
                           },
                        }).ToList()
            }
         };

         Response.ContentType = "application/rss+xml";

         await Response.Output.WriteAsync(rdoc.ToXml());

         return new EmptyResult();
      }

      [Route("rss/flight/{id}")]
      public async Task<ActionResult> flight(LevelOfFlight id)
      {
         var builds = await bModel.SelectFlight(id, RSS_SIZE, 0);


         RssDocument rdoc = new RssDocument()
         {
            Channel = new RssChannel()
            {
               Title = $"BuildFeed RSS - {id} Flight Level",
               Description = "",
               Generator = "BuildFeed.net RSS Controller",
               Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}"),
               SkipHours = new List<Hour>(),
               SkipDays = new List<Day>(),

               Items = (from build in builds
                        select new RssItem()
                        {
                           Title = build.FullBuildString,
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"
                           },
                        }).ToList()
            }
         };

         Response.ContentType = "application/rss+xml";

         await Response.Output.WriteAsync(rdoc.ToXml());

         return new EmptyResult();
      }

      [Route("rss/lab/{lab}")]
      public async Task<ActionResult> lab(string lab)
      {
         var builds = await bModel.SelectLab(lab, 0, RSS_SIZE);


         RssDocument rdoc = new RssDocument()
         {
            Channel = new RssChannel()
            {
               Title = $"BuildFeed RSS - {lab} Lab",
               Description = "",
               Generator = "BuildFeed.net RSS Controller",
               Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}"),
               SkipHours = new List<Hour>(),
               SkipDays = new List<Day>(),

               Items = (from build in builds
                        select new RssItem()
                        {
                           Title = build.FullBuildString,
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("viewBuild", new { controller = "front", id = build.Id })}"
                           },
                        }).ToList()
            }
         };

         Response.ContentType = "application/rss+xml";

         await Response.Output.WriteAsync(rdoc.ToXml());

         return new EmptyResult();
      }
   }
}