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
            return labs.ToArray();
        }

        public IEnumerable<SearchResult> GetSearchResult(string query)
        {
            List<SearchResult> results = new List<SearchResult>();

            var yearResults = from y in Build.SelectBuildYears()
                              where y.ToString().Contains(query)
                              orderby y descending
                              select new SearchResult()
                              {
                                  Url = Url.Route("Year Root", new { controller = "build", action = "year", year = y }),
                                  Label = y.ToString().Replace(query, "<strong>" + query + "</strong>"),
                                  Group = "Year"
                              };

            results.AddRange(yearResults);


            var labResults = from l in Build.SelectBuildLabs()
                             where l.Contains(query)
                             orderby l.IndexOf(query) ascending
                             select new SearchResult()
                             {
                                 Url = Url.Route("Lab Root", new { controller = "build", action = "lab", lab = l }),
                                 Label = l.Replace(query, "<strong>" + query + "</strong>"),
                                 Group = "Lab"
                             };

            results.AddRange(labResults);


            var buildResults = from b in Build.Select()
                               where b.FullBuildString.Contains(query)
                               orderby b.FullBuildString.IndexOf(query) ascending,
                                       b.BuildTime descending
                               select new SearchResult()
                               {
                                   Url = Url.Route("Actions", new { controller = "build", action = "info", id = b.Id }),
                                   Label = b.FullBuildString.Replace(query, "<strong>" + query + "</strong>"),
                                   Group = "Build"
                               };

            results.AddRange(buildResults);

            return results.Take(6);
        }
    }
}
