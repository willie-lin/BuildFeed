using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using BuildFeed.Code;
using BuildFeed.Local;
using BuildFeed.Models;
using BuildFeed.Models.ApiModel;
using BuildFeed.Models.ViewModel.Front;

namespace BuildFeed.Controllers
{
   public class ApiController : System.Web.Http.ApiController
   {
      private readonly Build _bModel;

      public ApiController() { _bModel = new Build(); }

      public async Task<BuildModel[]> GetBuilds(int limit = 20, int skip = 0)
      {
         List<BuildModel> builds = await _bModel.SelectBuildsByOrder(limit, skip);
         return builds.ToArray();
      }

      public async Task<FrontBuildGroup[]> GetBuildGroups(int limit = 20, int skip = 0)
      {
         FrontBuildGroup[] bgroups = await _bModel.SelectAllGroups(limit, skip);
         return bgroups.ToArray();
      }

      public async Task<BuildModel[]> GetBuildsForBuildGroup(uint major, uint minor, uint number, uint? revision = null)
      {
         List<BuildModel> builds = await _bModel.SelectGroup(new BuildGroup
         {
            Major = major,
            Minor = minor,
            Build = number,
            Revision = revision
         });

         return builds.ToArray();
      }

      public async Task<BuildModel[]> GetBuildsByLab(string lab, int limit = 20, int skip = 0)
      {
         List<BuildModel> builds = await _bModel.SelectLab(lab, limit, skip);
         return builds.ToArray();
      }

      public async Task<IEnumerable<string>> GetWin10Labs()
      {
         List<string> labs = new List<string>();
         labs.AddRange(await _bModel.SelectLabsForVersion(6, 4));
         labs.AddRange(await _bModel.SelectLabsForVersion(10, 0));

         return labs.GroupBy(l => l).Select(l => l.Key).Where(l => l.All(c => c != '(')).ToArray();
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
            await _bModel.InsertAll(apiModel.NewBuilds.Select(nb => new BuildModel
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
               SourceType = TypeOfSource.PrivateLeak
            }));
            return true;
         }
         return false;
      }

      public async Task<IEnumerable<SearchResult>> GetSearchResult(string id)
      {
         if (string.IsNullOrWhiteSpace(id))
         {
            return new SearchResult[0];
         }

         List<SearchResult> results = new List<SearchResult>();

         IEnumerable<SearchResult> sourceResults = from s in Enum.GetValues(typeof(TypeOfSource)).Cast<TypeOfSource>().Select(s => new
         {
            Text = MvcExtensions.GetDisplayTextForEnum(s),
            Value = s
         })
                                                   where s.Text.ToLower().Contains(id.ToLower())
                                                   orderby s.Text.ToLower().IndexOf(id.ToLower(), StringComparison.Ordinal) ascending
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
                                                   };

         results.AddRange(sourceResults);


         IEnumerable<SearchResult> versionResults = from v in await _bModel.SelectAllVersions()
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
                                                    };

         results.AddRange(versionResults);


         IEnumerable<SearchResult> yearResults = from y in await _bModel.SelectAllYears()
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
                                                 };

         results.AddRange(yearResults);


         IEnumerable<SearchResult> labResults = from l in await _bModel.SearchLabs(id)
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
                                                };

         results.AddRange(labResults);


         IEnumerable<SearchResult> buildResults = from b in await _bModel.Select()
                                                  where b.FullBuildString.ToLower().Contains(id.ToLower())
                                                  orderby b.FullBuildString.ToLower().IndexOf(id.ToLower(), StringComparison.Ordinal) ascending, b.BuildTime descending
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
                                                  };

         results.AddRange(buildResults);


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