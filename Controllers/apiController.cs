using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BuildFeed.Models;
using BuildFeed.Models.ApiModel;

namespace BuildFeed.Controllers
{
    public class apiController : ApiController
    {
        public IEnumerable<Build> GetBuilds()
        {
            return Build.SelectInBuildOrder();
        }

        public IEnumerable<string> GetWin10Labs()
        {
            List<string> labs = new List<string>();
            labs.AddRange(Build.SelectBuildLabs(6, 4));
            labs.AddRange(Build.SelectBuildLabs(10, 0));

            return labs.GroupBy(l => l).Select(l => l.Key).ToArray();
        }

        public IEnumerable<SearchResult> GetSearchResult(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new SearchResult[0];
            }

            List<SearchResult> results = new List<SearchResult>();

            var sourceResults = from s in Enum.GetValues(typeof(BuildFeed.Models.TypeOfSource)).Cast<BuildFeed.Models.TypeOfSource>().Select(s => DisplayHelpers.GetDisplayTextForEnum(s))
                                where s.ToLower().Contains(query.ToLower())
                                orderby s.ToLower().IndexOf(query.ToLower()) ascending
                                select new SearchResult()
                                {
                                    Url = Url.Route("Source Root", new { controller = "build", action = "source", source = s }),
                                    Label = s.Replace(query, "<strong>" + query + "</strong>"),
                                    Title = s,
                                    Group = "Source"
                                };

            results.AddRange(sourceResults);


            var versionResults = from v in Build.SelectBuildVersions()
                                 where string.Format("{0}.{1}", v.Major, v.Minor).StartsWith(query)
                                 orderby v.Major descending, v.Minor descending
                                 select new SearchResult()
                                 {
                                     Url = Url.Route("Version Root", new { controller = "build", action = "version", major = v.Major, minor = v.Minor }),
                                     Label = string.Format("{0}.{1}", v.Major, v.Minor).Replace(query, "<strong>" + query + "</strong>"),
                                     Title = "",
                                     Group = "Version"
                                 };

            results.AddRange(versionResults);


            var yearResults = from y in Build.SelectBuildYears()
                              where y.ToString().Contains(query)
                              orderby y descending
                              select new SearchResult()
                              {
                                  Url = Url.Route("Year Root", new { controller = "build", action = "year", year = y }),
                                  Label = y.ToString().Replace(query, "<strong>" + query + "</strong>"),
                                  Title = "",
                                  Group = "Year"
                              };

            results.AddRange(yearResults);


            var labResults = from l in Build.SelectBuildLabs()
                             where l.ToLower().Contains(query.ToLower())
                             orderby l.ToLower().IndexOf(query.ToLower()) ascending
                             select new SearchResult()
                             {
                                 Url = Url.Route("Lab Root", new { controller = "build", action = "lab", lab = l }),
                                 Label = l.Replace(query, "<strong>" + query + "</strong>"),
                                 Title = l,
                                 Group = "Lab"
                             };

            results.AddRange(labResults);


            var buildResults = from b in Build.Select()
                               where b.FullBuildString.ToLower().Contains(query.ToLower())
                               orderby b.FullBuildString.ToLower().IndexOf(query.ToLower()) ascending,
                                       b.BuildTime descending
                               select new SearchResult()
                               {
                                   Url = Url.Route("Actions", new { controller = "build", action = "info", id = b.Id }),
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

            return results.Take(6);
        }
    }
}
