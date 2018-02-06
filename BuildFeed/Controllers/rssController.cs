using System;
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

        public RssController()
        {
            _bModel = new BuildRepository();
        }

        [Route("rss/compiled")]
        public async Task<ActionResult> Index()
        {
            var builds = await _bModel.SelectBuildsByCompileDate(RSS_SIZE);

            var feed = new Feed
            {
                Title = "BuildFeed RSS - Recently Compiled",
                Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
                Items = (from build in builds
                    select new Item
                    {
                        Title = build.AlternateBuildString,
                        Link = new Uri(
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink =
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = build.RssCategories,
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
            var builds = await _bModel.SelectBuildsByAddedDate(RSS_SIZE);

            var feed = new Feed
            {
                Title = "BuildFeed RSS - Recently Added",
                Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
                Items = (from build in builds
                    select new Item
                    {
                        Title = build.AlternateBuildString,
                        Link = new Uri(
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink =
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = build.RssCategories,
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
            var builds = await _bModel.SelectBuildsByLeakedDate(RSS_SIZE);

            var feed = new Feed
            {
                Title = "BuildFeed RSS - Recently Leaked",
                Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
                Items = (from build in builds
                    select new Item
                    {
                        Title = build.AlternateBuildString,
                        Link = new Uri(
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink =
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = build.RssCategories,
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
            var builds = await _bModel.SelectBuildsByOrder(RSS_SIZE);

            var feed = new Feed
            {
                Title = "BuildFeed RSS - Highest Version",
                Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
                Items = (from build in builds
                    select new Item
                    {
                        Title = build.AlternateBuildString,
                        Link = new Uri(
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink =
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = build.RssCategories
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
            var builds = await _bModel.SelectLab(lab, RSS_SIZE);

            var feed = new Feed
            {
                Title = $"BuildFeed RSS - {lab} Lab",
                Link = new Uri($"{Request.Url.Scheme}://{Request.Url.Authority}"),
                Items = (from build in builds
                    select new Item
                    {
                        Title = build.AlternateBuildString,
                        Link = new Uri(
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}"),
                        Permalink =
                            $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action(nameof(FrontController.ViewBuild), new { controller = "Front", id = build.Id })}",
                        Categories = build.RssCategories
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