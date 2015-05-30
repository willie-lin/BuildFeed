using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using BuildFeed.Models;
using BuildFeed.Models.ViewModel;
using System.Xml.Linq;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace BuildFeed.Controllers
{
    public class supportController : Controller
    {
        [Route("login/")]
        public ActionResult login()
        {
            return View();
        }

        [HttpPost, Route("login/")]
        public ActionResult login(LoginUser ru)
        {
            if (ModelState.IsValid)
            {
                bool isAuthenticated = Membership.ValidateUser(ru.UserName, ru.Password);

                if (isAuthenticated)
                {
                    int expiryLength = ru.RememberMe ? 129600 : 60;
                    var ticket = new FormsAuthenticationTicket(ru.UserName, true, expiryLength);
                    var encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var cookieTicket = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    cookieTicket.Expires = DateTime.Now.AddMinutes(expiryLength);
                    cookieTicket.Path = FormsAuthentication.FormsCookiePath;
                    Response.Cookies.Add(cookieTicket);

                    string returnUrl = string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]) ? "/" : Request.QueryString["ReturnUrl"];

                    return Redirect(returnUrl);
                }
            }

            ViewData["ErrorMessage"] = "The username and password are not valid.";
            return View(ru);
        }

        [Authorize, Route("password/")]
        public ActionResult password()
        {
            return View();
        }

        [HttpPost, Authorize, Route("password/")]
        public ActionResult password(ChangePassword cp)
        {
            if (ModelState.IsValid)
            {
                var user = Membership.GetUser();
                bool success = user.ChangePassword(cp.OldPassword, cp.NewPassword);

                if (success)
                {
                    return Redirect("/");
                }
            }

            ViewData["ErrorMessage"] = "There was an error changing your password.";
            return View(cp);
        }

        [Route("logout/")]
        public ActionResult logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("/");
        }

        [Route("register/")]
        public ActionResult register()
        {
            return View();
        }

        [HttpPost, Route("register/")]
        public ActionResult register(RegistrationUser ru)
        {
            if (ModelState.IsValid)
            {
                MembershipCreateStatus status;
                Membership.CreateUser(ru.UserName, ru.Password, ru.EmailAddress, "THIS WILL BE IGNORED", "I WILL BE IGNORED", false, out status);

                switch (status)
                {
                    case MembershipCreateStatus.Success:
                        return RedirectToAction("thanks_register");
                    case MembershipCreateStatus.InvalidPassword:
                        ViewData["ErrorMessage"] = "The password is invalid.";
                        break;
                    case MembershipCreateStatus.DuplicateEmail:
                        ViewData["ErrorMessage"] = "A user account with this email address already exists.";
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        ViewData["ErrorMessage"] = "A user account with this user name already exists.";
                        break;
                    default:
                        ViewData["ErrorMessage"] = "Unspecified error.";
                        break;
                }
            }

            return View(ru);
        }

        [Route("register/")]
        public ActionResult thanks_register()
        {
            return View();
        }

        [Route("rss")]
        public ActionResult rss()
        {
            ViewBag.Labs = Build.SelectBuildLabs();
            return View();
        }

        [Route("sitemap/")]
#if !DEBUG
        [OutputCache(Duration = 3600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult sitemap()
        {
            IEnumerable<Build> builds = Build.SelectInVersionOrder();
            Dictionary<string, SitemapPagedAction[]> actions = new Dictionary<string, SitemapPagedAction[]>();

            actions.Add("Pages", new SitemapPagedAction[] { new SitemapPagedAction()
            {
                UrlParams = new RouteValueDictionary(new {
                    controller = "build",
                    action = "index",
                    page = 1
                }),
                Pages = (builds.Count() + (frontController._pageSize - 1)) / frontController._pageSize
            } });

            actions.Add("Versions", (from b in builds
                                     group b by new BuildVersion() { Major = b.MajorVersion, Minor = b.MinorVersion } into bv
                                     orderby bv.Key.Major descending,
                                             bv.Key.Minor descending
                                     select new SitemapPagedAction()
                                     {
                                         Name = string.Format("Windows NT {0}.{1}", bv.Key.Major, bv.Key.Minor),
                                         UrlParams = new RouteValueDictionary(new
                                         {
                                             controller = "front",
                                             action = "viewVersion",
                                             major = bv.Key.Major,
                                             minor = bv.Key.Minor,
                                             page = 1
                                         }),
                                         Pages = (bv.Count() + (frontController._pageSize - 1)) / frontController._pageSize
                                     }).ToArray());

            actions.Add("Labs", (from b in builds
                                 where !string.IsNullOrEmpty(b.Lab)
                                 group b by b.Lab into bv
                                 orderby bv.Key
                                 select new SitemapPagedAction()
                                 {
                                     Name = bv.Key,
                                     UrlParams = new RouteValueDictionary(new
                                     {
                                         controller = "front",
                                         action = "viewLab",
                                         lab = bv.Key,
                                         page = 1
                                     }),
                                     Pages = (bv.Count() + (frontController._pageSize - 1)) / frontController._pageSize
                                 }).ToArray());

            actions.Add("Years", (from b in builds
                                  where b.BuildTime.HasValue
                                  group b by b.BuildTime.Value.Year into bv
                                  orderby bv.Key descending
                                  select new SitemapPagedAction()
                                  {
                                      Name = bv.Key.ToString(),
                                      UrlParams = new RouteValueDictionary(new
                                      {
                                          controller = "front",
                                          action = "viewYear",
                                          year = bv.Key,
                                          page = 1
                                      }),
                                      Pages = (bv.Count() + (frontController._pageSize - 1)) / frontController._pageSize
                                  }).ToArray());

            actions.Add("Sources", (from b in builds
                                    group b by b.SourceType into bv
                                    orderby bv.Key
                                    select new SitemapPagedAction()
                                    {
                                        Name = DisplayHelpers.GetDisplayTextForEnum(bv.Key),
                                        UrlParams = new RouteValueDictionary(new
                                        {
                                            controller = "front",
                                            action = "viewSource",
                                            source = bv.Key,
                                            page = 1
                                        }),
                                        Pages = (bv.Count() + (frontController._pageSize - 1)) / frontController._pageSize
                                    }).ToArray());

            SitemapData model = new SitemapData()
            {
                Builds = (from b in Build.Select()
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
                          select new SitemapDataBuildGroup()
                          {
                              Id = bg.Key,
                              Builds = (from bgb in bg
                                        select new SitemapDataBuild()
                                        {
                                            Id = bgb.Id,
                                            Name = bgb.FullBuildString
                                        }).ToArray()
                          }).ToArray(),

                Actions = actions,
                Labs = (from b in builds
                        group b by b.Lab into lab
                        select lab.Key).ToArray()
            };

            return View(model);
        }

        [Route("xml-sitemap/")]
#if !DEBUG
        [OutputCache(Duration = 3600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult xmlsitemap()
        {
            XNamespace xn = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
            List<XElement> xlist = new List<XElement>();

            // home page
            XElement home = new XElement(xn + "url");
            home.Add(new XElement(xn + "loc", Request.Url.GetLeftPart(UriPartial.Authority) + "/"));
            home.Add(new XElement(xn + "changefreq", "daily"));
            xlist.Add(home);

            foreach (var b in Build.Select())
            {
                XElement url = new XElement(xn + "url");
                url.Add(new XElement(xn + "loc", Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("viewBuild", "front", new { id = b.Id })));
                if (b.Modified != DateTime.MinValue)
                {
                    url.Add(new XElement(xn + "lastmod", b.Modified.ToString("yyyy-MM-dd")));
                }
                xlist.Add(url);
            }

            XDeclaration decl = new XDeclaration("1.0", "utf-8", "");
            XElement root = new XElement(xn + "urlset", xlist);

            XDocument xdoc = new XDocument(decl, root);

            Response.ContentType = "application/xml";
            xdoc.Save(Response.OutputStream);


            return new EmptyResult();
        }

        [Route("statistics/")]
#if !DEBUG
        [OutputCache(Duration = 3600, VaryByParam = "none", VaryByCustom = "userName")]
#endif
        public ActionResult stats()
        {
            var builds = Build.Select();

            List<MonthCount> Additions = new List<MonthCount>();
            var rawAdditions = from b in builds
                               where b.Added > DateTime.Now.AddYears(-1)
                               group b by new { Year = b.Added.Year, Week = Convert.ToInt32(Math.Floor(b.Added.DayOfYear / 7m)) } into bm
                               select new MonthCount()
                               {
                                   Month = bm.Key.Week,
                                   Year = bm.Key.Year,
                                   Count = bm.Count()
                               };

            for (int i = -52; i <= 0; i++)
            {
                DateTime dt = DateTime.Now.AddDays(i * 7);
                Additions.Add(new MonthCount()
                {
                    Month = Convert.ToInt32(Math.Floor(dt.DayOfYear / 7m)),
                    Year = dt.Year,
                    Count = rawAdditions.SingleOrDefault(a => a.Month == Convert.ToInt32(Math.Floor(dt.DayOfYear / 7m)) && a.Year == dt.Year).Count
                });
            }

            List<MonthCount> Compiles = new List<MonthCount>();
            var rawCompiles = from b in builds
                              where b.BuildTime.HasValue
                              group b by new { Year = b.BuildTime.Value.Year, Month = Convert.ToInt32(Math.Floor((b.BuildTime.Value.Month - 0.1m) / 3m) * 3) + 1 } into bm
                              select new MonthCount()
                              {
                                  Month = bm.Key.Month,
                                  Year = bm.Key.Year,
                                  Count = bm.Count()
                              };


            var rawLabCounts = from bl in (from b in builds
                                           where !string.IsNullOrEmpty(b.Lab)
                                           group b by b.Lab into bl
                                           select bl)
                               where bl.Count() > 49
                               orderby bl.Count() descending
                               select new Tuple<string, int>(bl.Key, bl.Count());

            StatsPage m = new StatsPage()
            {
                AdditionsByMonth = Additions,
                CompilesByMonth = rawCompiles.OrderBy(r => r.Year).ThenBy(r => r.Month),
                BuildsByLab = rawLabCounts
            };

            return View(m);
        }
    }
}