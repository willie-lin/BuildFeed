using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BuildFeed.Code;
using BuildFeed.Models;
using X.Web.RSS;
using X.Web.RSS.Enumerators;
using X.Web.RSS.Structure;
using X.Web.RSS.Structure.Validators;

namespace BuildFeed.Controllers
{
   public class RssController : LocalController
   {
      private const int RSS_SIZE = 25;
      private readonly Build _bModel;

      public RssController() { _bModel = new Build(); }

      [Route("rss/compiled")]
      public async Task<ActionResult> Index()
      {
         var builds = await _bModel.SelectBuildsByCompileDate(RSS_SIZE, 0);

         RssDocument rdoc = new RssDocument
                            {
                               Channel = new RssChannel
                                         {
                                            Title = "BuildFeed RSS - Recently Compiled",
                                            Description = "",
                                            Generator = "BuildFeed.net RSS Controller",
                                            Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}"),
                                            SkipHours = new List<Hour>(),
                                            SkipDays = new List<Day>(),
                                            Items = (from build in builds
                                                     select new RssItem
                                                            {
                                                               Title = build.FullBuildString,
                                                               Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"),
                                                               Guid = new RssGuid
                                                                      {
                                                                         IsPermaLink = true,
                                                                         Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"
                                                                      }
                                                            }).ToList()
                                         }
                            };

         return new ContentResult
                {
                   Content = rdoc.ToXml(),
                   ContentType = "application/rss+xml",
                   ContentEncoding = Encoding.UTF8
                };
      }

      [Route("rss/added")]
      public async Task<ActionResult> Added()
      {
         var builds = await _bModel.SelectBuildsByAddedDate(RSS_SIZE, 0);

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
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"
                           },
                           Category = new RssCategory() { Text = build.Family.ToString() },
                           InternalPubDate = new RssDate(build.Added).DateStringISO8601 // bit of a dirty hack to work around problem in X.Web.RSS with the date format.
                        }).ToList()
            }
         };

         return new ContentResult
         {
            Content = rdoc.ToXml(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/leaked")]
      public async Task<ActionResult> Leaked()
      {
         var builds = await _bModel.SelectBuildsByLeakedDate(RSS_SIZE, 0);

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
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"
                           },
                           InternalPubDate = new RssDate(build.LeakDate.Value).DateStringISO8601 // bit of a dirty hack to work around problem in X.Web.RSS with the date format.
                        }).ToList()
            }
         };

         return new ContentResult
         {
            Content = rdoc.ToXml(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/version")]
      public async Task<ActionResult> Version()
      {
         var builds = await _bModel.SelectBuildsByOrder(RSS_SIZE, 0);


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
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"
                           },
                        }).ToList()
            }
         };

         return new ContentResult
         {
            Content = rdoc.ToXml(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/flight/{id}")]
      public async Task<ActionResult> Flight(LevelOfFlight id)
      {
         var builds = await _bModel.SelectFlight(id, RSS_SIZE, 0);

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
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"
                           },
                        }).ToList()
            }
         };

         return new ContentResult
         {
            Content = rdoc.ToXml(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }

      [Route("rss/lab/{lab}")]
      public async Task<ActionResult> Lab(string lab)
      {
         var builds = await _bModel.SelectLab(lab, RSS_SIZE, 0);

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
                           Link = new RssUrl($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"),
                           Guid = new RssGuid()
                           {
                              IsPermaLink = true,
                              Value = $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("ViewBuild", new { controller = "Front", id = build.Id })}"
                           },
                        }).ToList()
            }
         };

         return new ContentResult
         {
            Content = rdoc.ToXml(),
            ContentType = "application/rss+xml",
            ContentEncoding = Encoding.UTF8
         };
      }
   }
}