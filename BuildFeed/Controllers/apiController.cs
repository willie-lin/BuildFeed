using BuildFeed.Local;
using BuildFeed.Models;
using BuildFeed.Models.ApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;

namespace BuildFeed.Controllers
{
   public class apiController : ApiController
   {
      private Build bModel;

      public apiController() : base()
      {
         bModel = new Build();
      }

      public async Task<BuildModel[]> GetBuilds(int skip, int limit = 20)
      {
         var builds = await bModel.SelectInBuildOrder(limit, skip);
         return builds.ToArray();
      }

      public async Task<IEnumerable<string>> GetWin10Labs()
      {
         List<string> labs = new List<string>();
         labs.AddRange(await bModel.SelectBuildLabs(6, 4));
         labs.AddRange(await bModel.SelectBuildLabs(10, 0));

         return labs
            .GroupBy(l => l)
            .Select(l => l.Key)
            .Where(l => l.All(c => c != '('))
            .ToArray();
      }

      [HttpPost]
      public async Task<bool> AddWin10Builds(NewBuild apiModel)
      {
         if (apiModel == null)
         {
            return false;
         }
         if (Membership.ValidateUser(apiModel.Username, apiModel.Password))
         {
            await bModel.InsertAll(apiModel.NewBuilds.Select(nb => new BuildModel()
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

      public async Task<IEnumerable<SearchResult>> GetSearchResult(string id)
      {
         if (string.IsNullOrWhiteSpace(id))
         {
            return new SearchResult[0];
         }

         List<SearchResult> results = new List<SearchResult>();

         var sourceResults = from s in Enum.GetValues(typeof(TypeOfSource)).Cast<TypeOfSource>().Select(s => new { Text = DisplayHelpers.GetDisplayTextForEnum(s), Value = s })
                             where s.Text.ToLower().Contains(id.ToLower())
                             orderby s.Text.ToLower().IndexOf(id.ToLower(), StringComparison.Ordinal) ascending
                             select new SearchResult()
                             {
                                Url = Url.Route("Source Root", new { controller = "front", action = "viewSource", source = s.Value }),
                                Label = s.Text.Replace(id, "<strong>" + id + "</strong>"),
                                Title = s.Text,
                                Group = Common.SearchSource
                             };

         results.AddRange(sourceResults);


         var versionResults = from v in await bModel.SelectBuildVersions()
                              where $"{v.Major}.{v.Minor}".StartsWith(id)
                              orderby v.Major descending, v.Minor descending
                              select new SearchResult()
                              {
                                 Url = Url.Route("Version Root", new { controller = "front", action = "viewVersion", major = v.Major, minor = v.Minor }),
                                 Label = $"{v.Major}.{v.Minor}".Replace(id, "<strong>" + id + "</strong>"),
                                 Title = "",
                                 Group = Common.SearchVersion
                              };

         results.AddRange(versionResults);


         var yearResults = from y in await bModel.SelectBuildYears()
                           where y.ToString().Contains(id)
                           orderby y descending
                           select new SearchResult()
                           {
                              Url = Url.Route("Year Root", new { controller = "front", action = "viewYear", year = y }),
                              Label = y.ToString().Replace(id, "<strong>" + id + "</strong>"),
                              Title = "",
                              Group = Common.SearchYear
                           };

         results.AddRange(yearResults);


         var labResults = from l in await bModel.SearchBuildLabs(id)
                          orderby l.IndexOf(id.ToLower()) ascending,
                                  l.Length ascending
                          select new SearchResult()
                          {
                             Url = Url.Route("Lab Root", new { controller = "front", action = "viewLab", lab = l.Replace('/', '-') }),
                             Label = l.Replace(id, $"<strong>{id}</strong>"),
                             Title = l,
                             Group = Common.SearchLab
                          };

         results.AddRange(labResults);


         var buildResults = from b in await bModel.Select()
                            where b.FullBuildString.ToLower().Contains(id.ToLower())
                            orderby b.FullBuildString.ToLower().IndexOf(id.ToLower(), StringComparison.Ordinal) ascending,
                                    b.BuildTime descending
                            select new SearchResult()
                            {
                               Url = Url.Route("Build", new { controller = "front", action = "viewBuild", id = b.Id }),
                               Label = b.FullBuildString.Replace(id, $"<strong>{id}</strong>"),
                               Title = b.FullBuildString,
                               Group = Common.SearchBuild
                            };

         results.AddRange(buildResults);


         if (results.Count == 0)
         {
            results.Add(new SearchResult()
            {
               Url = "/",
               Label = Common.SearchEmpty,
               Group = ""
            });
         }

         return results.Take(15);
      }
   }
}
