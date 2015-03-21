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
        [Route("rss/compiled")]
        public async Task<ActionResult> index()
        {
            var builds = Build.SelectInBuildOrder().Take(20);

            RssDocument rdoc = new RssDocument()
            {
                Channel = new RssChannel()
                {
                    Title = "BuildFeed RSS - Recently Compiled",
                    Description = "",
                    Generator = "BuildFeed.net RSS Controller",
                    Link = new RssUrl(string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority)),
                    SkipHours = new List<Hour>(),
                    SkipDays = new List<Day>(),

                    Items = (from build in builds
                             select new RssItem()
                             {
                                 Title = build.FullBuildString,
                                 Link = new RssUrl(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id }))),
                                 Guid = new RssGuid() { IsPermaLink = true, Value = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id })) },
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
            var builds = Build.Select().OrderByDescending(b => b.Added).Take(20);

            RssDocument rdoc = new RssDocument()
            {
                Channel = new RssChannel()
                {
                    Title = "BuildFeed RSS - Recently Added",
                    Description = "",
                    Generator = "BuildFeed.net RSS Controller",
                    Link = new RssUrl(string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority)),
                    SkipHours = new List<Hour>(),
                    SkipDays = new List<Day>(),

                    Items = (from build in builds
                             select new RssItem()
                             {
                                 Title = build.FullBuildString,
                                 Link = new RssUrl(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id }))),
                                 Guid = new RssGuid() { IsPermaLink = true, Value = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id })) },
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
            var builds = Build.Select().Where(b => b.LeakDate.HasValue).OrderByDescending(b => b.LeakDate.Value).Take(20);

            RssDocument rdoc = new RssDocument()
            {
                Channel = new RssChannel()
                {
                    Title = "BuildFeed RSS - Recently Leaked",
                    Description = "",
                    Generator = "BuildFeed.net RSS Controller",
                    Link = new RssUrl(string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority)),
                    SkipHours = new List<Hour>(),
                    SkipDays = new List<Day>(),

                    Items = (from build in builds
                             select new RssItem()
                             {
                                 Title = build.FullBuildString,
                                 Link = new RssUrl(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id }))),
                                 Guid = new RssGuid() { IsPermaLink = true, Value = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id })) },
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
            var builds = Build.SelectInVersionOrder()
                .Take(20);


            RssDocument rdoc = new RssDocument()
            {
                Channel = new RssChannel()
                {
                    Title = "BuildFeed RSS - Highest Version",
                    Description = "",
                    Generator = "BuildFeed.net RSS Controller",
                    Link = new RssUrl(string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority)),
                    SkipHours = new List<Hour>(),
                    SkipDays = new List<Day>(),

                    Items = (from build in builds
                             select new RssItem()
                             {
                                 Title = build.FullBuildString,
                                 Link = new RssUrl(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id }))),
                                 Guid = new RssGuid() { IsPermaLink = true, Value = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id })) },
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
            var builds = Build.SelectInBuildOrder()
                .Where(b => b.FlightLevel == id)
                .Take(20);


            RssDocument rdoc = new RssDocument()
            {
                Channel = new RssChannel()
                {
                    Title = string.Format("BuildFeed RSS - {0} Flight Level", id),
                    Description = "",
                    Generator = "BuildFeed.net RSS Controller",
                    Link = new RssUrl(string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority)),
                    SkipHours = new List<Hour>(),
                    SkipDays = new List<Day>(),

                    Items = (from build in builds
                             select new RssItem()
                             {
                                 Title = build.FullBuildString,
                                 Link = new RssUrl(string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id }))),
                                 Guid = new RssGuid() { IsPermaLink = true, Value = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Action("info", new { controller = "Build", id = build.Id })) },
                             }).ToList()
                }
            };

            Response.ContentType = "application/rss+xml";

            await Response.Output.WriteAsync(rdoc.ToXml());

            return new EmptyResult();
        }
    }
}