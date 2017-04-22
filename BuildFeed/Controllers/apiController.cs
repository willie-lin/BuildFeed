using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using BuildFeed.Code;
using BuildFeed.Local;
using BuildFeed.Model;
using BuildFeed.Model.Api;
using BuildFeed.Model.View;
using MongoDB.Bson;
using OneSignal.CSharp.SDK;

#pragma warning disable SG0016 // Controller method is vulnerable to CSRF - Not relevant for API

namespace BuildFeed.Controllers
{
    public class ApiController : System.Web.Http.ApiController
    {
        private readonly BuildRepository _bModel;

        public ApiController()
        {
            _bModel = new BuildRepository();
        }

        public async Task<ApiBuild[]> GetBuilds(int limit = 20, int skip = 0)
        {
            List<Build> builds = await _bModel.SelectBuildsByOrder(limit, skip);
            return (from b in builds
                   select new ApiBuild
                   {
                       Id = b.Id,

                       MajorVersion = b.MajorVersion,
                       MinorVersion = b.MinorVersion,
                       Number = b.Number,
                       Revision = b.Revision,

                       Lab = b.Lab,
                       BuildTime = b.BuildTime,

                       SourceType = b.SourceType,
                       SourceDetails = b.SourceDetails,
                       LeakDate = b.LeakDate,

                       FullBuildString = b.FullBuildString,
                       AlternateBuildString = b.AlternateBuildString,
                       LabUrl = b.LabUrl,

                       Added = b.Added,
                       Modified = b.Modified
                   }).ToArray();
        }

        public async Task<FrontBuildGroup[]> GetBuildGroups(int limit = 20, int skip = 0)
        {
            FrontBuildGroup[] bgroups = await _bModel.SelectAllGroups(limit, skip);
            return bgroups.ToArray();
        }

        public async Task<ApiBuild[]> GetBuildsForBuildGroup(uint major, uint minor, uint number, uint? revision = null)
        {
            List<Build> builds = await _bModel.SelectGroup(new BuildGroup
            {
                Major = major,
                Minor = minor,
                Build = number,
                Revision = revision
            });

            return (from b in builds
                    select new ApiBuild
                    {
                        Id = b.Id,

                        MajorVersion = b.MajorVersion,
                        MinorVersion = b.MinorVersion,
                        Number = b.Number,
                        Revision = b.Revision,

                        Lab = b.Lab,
                        BuildTime = b.BuildTime,

                        SourceType = b.SourceType,
                        SourceDetails = b.SourceDetails,
                        LeakDate = b.LeakDate,

                        FullBuildString = b.FullBuildString,
                        AlternateBuildString = b.AlternateBuildString,
                        LabUrl = b.LabUrl,

                        Added = b.Added,
                        Modified = b.Modified
                    }).ToArray();
        }

        public async Task<ApiBuild[]> GetBuildsByLab(string lab, int limit = 20, int skip = 0)
        {
            List<Build> builds = await _bModel.SelectLab(lab, limit, skip);

            return (from b in builds
                    select new ApiBuild
                    {
                        Id = b.Id,

                        MajorVersion = b.MajorVersion,
                        MinorVersion = b.MinorVersion,
                        Number = b.Number,
                        Revision = b.Revision,

                        Lab = b.Lab,
                        BuildTime = b.BuildTime,

                        SourceType = b.SourceType,
                        SourceDetails = b.SourceDetails,
                        LeakDate = b.LeakDate,

                        FullBuildString = b.FullBuildString,
                        AlternateBuildString = b.AlternateBuildString,
                        LabUrl = b.LabUrl,

                        Added = b.Added,
                        Modified = b.Modified
                    }).ToArray();
        }

        public async Task<List<FamilyOverview>> GetFamilyOverviews()
        {
            return await _bModel.SelectFamilyOverviews();
        }

        public async Task<IEnumerable<string>> GetWin10Labs()
        {
            var labs = new List<string>();
            labs.AddRange(await _bModel.SelectLabsForVersion(6, 4));
            labs.AddRange(await _bModel.SelectLabsForVersion(10, 0));

            return labs.GroupBy(l => l).Select(l => l.Key).Where(l => l.All(c => c != '(')).ToArray();
        }

        [HttpPost]
        public async Task<bool> AddWin10Builds(NewBuildPost apiModel)
        {
            if (apiModel == null)
            {
                return false;
            }
            if (Membership.ValidateUser(apiModel.Username, apiModel.Password) && (Roles.IsUserInRole(apiModel.Username, "Editors") || Roles.IsUserInRole(apiModel.Username, "Administrators")))
            {
                var generateOldItem = new Func<NewBuild, BuildDetails>(nb =>
                {
                    BuildDetails bi = new BuildDetails
                    {
                        MajorVersion = nb.MajorVersion,
                        MinorVersion = nb.MinorVersion,
                        Number = nb.Number,
                        Revision = nb.Revision,
                        Lab = nb.Lab,
                        BuildTime = nb.BuildTime.HasValue
                            ? DateTime.SpecifyKind(nb.BuildTime.Value, DateTimeKind.Utc)
                            : null as DateTime?,
                        SourceType = TypeOfSource.PrivateLeak
                    };

                    return bi;
                });

                IEnumerable<Build> builds = apiModel.NewBuilds.Select(nb => new Build
                {
                    MajorVersion = nb.MajorVersion,
                    MinorVersion = nb.MinorVersion,
                    Number = nb.Number,
                    Revision = nb.Revision,
                    Lab = nb.Lab,
                    BuildTime = nb.BuildTime.HasValue
                        ? DateTime.SpecifyKind(nb.BuildTime.Value, DateTimeKind.Utc)
                        : null as DateTime?,
                    Added = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    Modified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    SourceType = TypeOfSource.PrivateLeak,

                    History = new List<ItemHistory<BuildDetails>>
                    {
                        new ItemHistory<BuildDetails>
                        {
                            Type = ItemHistoryType.Added,
                            Time = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                            UserName = apiModel.Username,
                            Item = generateOldItem(nb)
                        }
                    }
                });

                foreach (Build build in builds)
                {
                    await _bModel.Insert(build);

                    OneSignalClient osc = new OneSignalClient(ConfigurationManager.AppSettings["push:OneSignalApiKey"]);
                    osc.PushNewBuild(build,
                        $"https://buildfeed.net{Url.Route("Build", new { controller = "Front", action = nameof(FrontController.ViewBuild), id = build.Id, area = "", httproute = "" })}?utm_source=notification&utm_campaign=new_build");
                }
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<SearchResult>> GetSearchResult(string id, int maxResults = 16)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new SearchResult[0];
            }

            var results = new List<SearchResult>();

            results.AddRange(from s in (from c in await _bModel.SelectAllSources()
                                        select new
                                        {
                                            Text = MvcExtensions.GetDisplayTextForEnum(c),
                                            Value = c
                                        })
                             where s.Text.ToLower().Contains(id.ToLower())
                             orderby s.Text.ToLower().IndexOf(id.ToLower(), StringComparison.Ordinal)
                             select new SearchResult
                             {
                                 Url = Url.Route("Source Root",
                                     new
                                     {
                                         controller = "Front",
                                         action = "ViewSource",
                                         source = s.Value
                                     }),
                                 Label = s.Text.Replace(id, "<strong>" + id + "</strong>"),
                                 Title = s.Text,
                                 Group = VariantTerms.Search_Source
                             });

            if (results.Count >= maxResults)
            {
                return results.Take(maxResults);
            }

            results.AddRange(from v in await _bModel.SelectAllVersions()
                             where $"{v.Major}.{v.Minor}".StartsWith(id)
                             orderby v.Major descending, v.Minor descending
                             select new SearchResult
                             {
                                 Url = Url.Route("Version Root",
                                     new
                                     {
                                         controller = "Front",
                                         action = "ViewVersion",
                                         major = v.Major,
                                         minor = v.Minor
                                     }),
                                 Label = $"{v.Major}.{v.Minor}".Replace(id, "<strong>" + id + "</strong>"),
                                 Title = "",
                                 Group = VariantTerms.Search_Version
                             });

            if (results.Count >= maxResults)
            {
                return results.Take(maxResults);
            }

            results.AddRange(from y in await _bModel.SelectAllYears()
                             where y.ToString().Contains(id)
                             orderby y descending
                             select new SearchResult
                             {
                                 Url = Url.Route("Year Root",
                                     new
                                     {
                                         controller = "Front",
                                         action = "ViewYear",
                                         year = y
                                     }),
                                 Label = y.ToString().Replace(id, "<strong>" + id + "</strong>"),
                                 Title = "",
                                 Group = VariantTerms.Search_Year
                             });

            if (results.Count >= maxResults)
            {
                return results.Take(maxResults);
            }

            results.AddRange(from l in await _bModel.SearchLabs(id)
                             select new SearchResult
                             {
                                 Url = Url.Route("Lab Root",
                                     new
                                     {
                                         controller = "Front",
                                         action = "ViewLab",
                                         lab = l.Replace('/', '-')
                                     }),
                                 Label = l.Replace(id, $"<strong>{id}</strong>"),
                                 Title = l,
                                 Group = VariantTerms.Search_Lab
                             });

            if (results.Count >= maxResults)
            {
                return results.Take(maxResults);
            }

            results.AddRange(from b in await _bModel.SelectBuildsByStringSearch(id, maxResults)
                             select new SearchResult
                             {
                                 Url = Url.Route("Build",
                                     new
                                     {
                                         controller = "Front",
                                         action = "ViewBuild",
                                         id = b.Id
                                     }),
                                 Label = b.FullBuildString.Replace(id, $"<strong>{id}</strong>"),
                                 Title = b.FullBuildString,
                                 Group = VariantTerms.Search_Build
                             });

            if (results.Count == 0)
            {
                results.Add(new SearchResult
                {
                    Url = "/",
                    Label = VariantTerms.Search_Empty,
                    Group = ""
                });
            }

            return results.Take(16);
        }
    }
}