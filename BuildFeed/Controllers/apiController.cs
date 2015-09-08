using BuildFeed.Models;
using BuildFeed.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Security;

namespace BuildFeed.Controllers
{
    public class apiController : ApiController
    {
        public IEnumerable<BuildModel> GetBuilds()
        {
            return new Build().SelectInBuildOrder();
        }

        public IEnumerable<string> GetWin10Labs()
        {
            Build b = new Build();
            List<string> labs = new List<string>();
            labs.AddRange(b.SelectBuildLabs(6, 4));
            labs.AddRange(b.SelectBuildLabs(10, 0));

            return labs.GroupBy(l => l).Select(l => l.Key).Where(l => l.All(c => c != '(')).ToArray();
        }

        [HttpPost]
        public bool AddWin10Builds(NewBuild apiModel)
        {
            if (apiModel == null)
            {
                return false;
            }
            if (Membership.ValidateUser(apiModel.Username, apiModel.Password))
            {
                new Build().InsertAll(apiModel.NewBuilds.Select(nb => new BuildModel()
                {
                    MajorVersion = nb.MajorVersion,
                    MinorVersion = nb.MinorVersion,
                    Number = nb.Number,
                    Revision = nb.Revision,
                    Lab = nb.Lab,
                    BuildTime = nb.BuildTime,
                    FlightLevel = nb.FlightLevel
                }));
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<SearchResult> GetSearchResult(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new SearchResult[0];
            }

            List<SearchResult> results = new List<SearchResult>();

            var sourceResults = from s in Enum.GetValues(typeof(TypeOfSource)).Cast<TypeOfSource>().Select(s => new { Text = DisplayHelpers.GetDisplayTextForEnum(s), Value = s })
                                where s.Text.ToLower().Contains(query.ToLower())
                                orderby s.Text.ToLower().IndexOf(query.ToLower(), StringComparison.Ordinal) ascending
                                select new SearchResult()
                                {
                                    Url = Url.Route("Source Root", new { controller = "front", action = "viewSource", source = s.Value }),
                                    Label = s.Text.Replace(query, "<strong>" + query + "</strong>"),
                                    Title = s.Text,
                                    Group = "Source"
                                };

            results.AddRange(sourceResults);


            var versionResults = from v in new Build().SelectBuildVersions()
                                 where $"{v.Major}.{v.Minor}".StartsWith(query)
                                 orderby v.Major descending, v.Minor descending
                                 select new SearchResult()
                                 {
                                     Url = Url.Route("Version Root", new { controller = "front", action = "viewVersion", major = v.Major, minor = v.Minor }),
                                     Label = $"{v.Major}.{v.Minor}".Replace(query, "<strong>" + query + "</strong>"),
                                     Title = "",
                                     Group = "Version"
                                 };

            results.AddRange(versionResults);


            var yearResults = from y in new Build().SelectBuildYears()
                              where y.ToString().Contains(query)
                              orderby y descending
                              select new SearchResult()
                              {
                                  Url = Url.Route("Year Root", new { controller = "front", action = "viewYear", year = y }),
                                  Label = y.ToString().Replace(query, "<strong>" + query + "</strong>"),
                                  Title = "",
                                  Group = "Year"
                              };

            results.AddRange(yearResults);


            var labResults = from l in new Build().SelectBuildLabs()
                             where l.ToLower().Contains(query.ToLower())
                             orderby l.ToLower().IndexOf(query.ToLower(), StringComparison.Ordinal) ascending
                             select new SearchResult()
                             {
                                 Url = Url.Route("Lab Root", new { controller = "front", action = "viewLab", lab = l }),
                                 Label = l.Replace(query, "<strong>" + query + "</strong>"),
                                 Title = l,
                                 Group = "Lab"
                             };

            results.AddRange(labResults);


            var buildResults = from b in new Build().Select()
                               where b.FullBuildString.ToLower().Contains(query.ToLower())
                               orderby b.FullBuildString.ToLower().IndexOf(query.ToLower(), StringComparison.Ordinal) ascending,
                                       b.BuildTime descending
                               select new SearchResult()
                               {
                                   Url = Url.Route("Build", new { controller = "front", action = "viewBuild", id = b.Id }),
                                   Label = b.FullBuildString.Replace(query, "<strong>" + query + "</strong>"),
                                   Title = b.FullBuildString,
                                   Group = "Build"
                               };

            results.AddRange(buildResults);


            if (results.Count == 0)
            {
                results.Add(new SearchResult()
                {
                    Url = "/",
                    Label = "No Results found",
                    Group = ""
                });
            }

            return results.Take(15);
        }
    }
}
