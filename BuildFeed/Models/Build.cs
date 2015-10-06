using BuildFeed.Local;
using BuildFeed.Models.ApiModel;
using BuildFeed.Models.ViewModel.Front;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Models
{
   [DataObject]
   public class BuildModel
   {
      [Key, BsonId]
      public Guid Id { get; set; }

      public long? LegacyId { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "MajorVersion")]
      public byte MajorVersion { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "MinorVersion")]
      public byte MinorVersion { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "Number")]
      public ushort Number { get; set; }

      [Display(ResourceType = typeof(Model), Name = "Revision")]
      [DisplayFormat(ConvertEmptyStringToNull = true)]
      public ushort? Revision { get; set; }

      [Display(ResourceType = typeof(Model), Name = "Lab")]
      public string Lab { get; set; }

      [Display(ResourceType = typeof(Model), Name = "BuildTime")]
      [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true, DataFormatString = "{0:yyMMdd-HHmm}")]
      public DateTime? BuildTime { get; set; }


      [@Required]
      [Display(ResourceType = typeof(Model), Name = "Added")]
      public DateTime Added { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "Modified")]
      public DateTime Modified { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "SourceType")]
      [EnumDataType(typeof(TypeOfSource))]
      public TypeOfSource SourceType { get; set; }

      [Display(ResourceType = typeof(Model), Name = "SourceDetails")]
      [AllowHtml]
      public string SourceDetails { get; set; }

      [Display(ResourceType = typeof(Model), Name = "LeakDate")]
      [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
      public DateTime? LeakDate { get; set; }

      [Display(ResourceType = typeof(Model), Name = "FlightLevel")]
      [EnumDataType(typeof(LevelOfFlight))]
      public LevelOfFlight FlightLevel { get; set; }

      public string LabUrl { get; set; }

      public bool IsLeaked
      {
         get
         {
            switch (SourceType)
            {
               case TypeOfSource.PublicRelease:
               case TypeOfSource.InternalLeak:
               case TypeOfSource.UpdateGDR:
               case TypeOfSource.UpdateLDR:
                  return true;
               default:
                  return false;
            }
         }
      }

      public string FullBuildString
      {
         get
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}.{1}.{2}", MajorVersion, MinorVersion, Number);

            if (Revision.HasValue)
            {
               sb.AppendFormat(".{0}", Revision);
            }

            if (!string.IsNullOrWhiteSpace(Lab))
            {
               sb.AppendFormat(".{0}", Lab);
            }

            if (BuildTime.HasValue)
            {
               sb.AppendFormat(".{0:yyMMdd-HHmm}", BuildTime);
            }

            return sb.ToString();
         }
      }

      public string GenerateLabUrl() => Lab.Replace('/', '-').ToLower();
   }

   public class Build
   {
      private const string _buildCollectionName = "builds";

      private MongoClient _dbClient;
      private IMongoCollection<BuildModel> _buildCollection;

      public Build()
      {
         _dbClient = new MongoClient(new MongoClientSettings()
         {
            Server = new MongoServerAddress(MongoConfig.Host, MongoConfig.Port)
         });

         _buildCollection = _dbClient.GetDatabase(MongoConfig.Database).GetCollection<BuildModel>(_buildCollectionName);
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<List<BuildModel>> Select()
      {
         return await _buildCollection.Find(new BsonDocument()).ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<List<BuildModel>> SelectLatest(int limit, int skip)
      {
         return await _buildCollection.Find(new BsonDocument())
            .SortByDescending(b => b.Added)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<List<BuildModel>> SelectLatestLeaked(int limit, int skip)
      {
         return await _buildCollection.Find(b => b.LeakDate != null)
            .SortByDescending(b => b.LeakDate)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<FrontBuildGroup>> SelectBuildGroups(int limit, int skip)
      {
         return await _buildCollection.Aggregate()
            .Group(b => new BuildGroup()
            {
               Major = b.MajorVersion,
               Minor = b.MinorVersion,
               Build = b.Number,
               Revision = b.Revision
            },
            bg => new FrontBuildGroup()
            {
               Key = bg.Key,
               BuildCount = bg.Count(),
               LastBuild = bg.Max(b => b.BuildTime)
            })
            .SortByDescending(b => b.Key.Major)
            .ThenByDescending(b => b.Key.Minor)
            .ThenByDescending(b => b.Key.Build)
            .ThenByDescending(b => b.Key.Revision)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<int> SelectBuildGroupsCount()
      {
         var pipeline = _buildCollection.Aggregate()
            .Group(b => new BuildGroup()
            {
               Major = b.MajorVersion,
               Minor = b.MinorVersion,
               Build = b.Number,
               Revision = b.Revision
            },
            bg => new BsonDocument());

         return (await pipeline.ToListAsync()).Count;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<Tuple<BuildGroup, List<BuildModel>>> SelectSingleBuildGroup(BuildGroup bGroup)
      {
         var pipeline = _buildCollection.Aggregate()
            .Match(b => b.MajorVersion == bGroup.Major)
            .Match(b => b.MinorVersion == bGroup.Minor)
            .Match(b => b.Number == bGroup.Build)
            .Match(b => b.Revision == bGroup.Revision)
            .SortByDescending(b => b.BuildTime);

         return new Tuple<BuildGroup, List<BuildModel>>(bGroup, await pipeline.ToListAsync());
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<BuildModel> SelectById(Guid id)
      {
         return await _buildCollection.Find(f => f.Id == id).SingleOrDefaultAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<BuildModel> SelectByLegacyId(long id)
      {
         return await _buildCollection.Find(f => f.LegacyId == id).SingleOrDefaultAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectInBuildOrder()
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ToListAsync();

      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectInBuildOrder(int limit, int skip)
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectInVersionOrder()
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ThenByDescending(b => b.BuildTime)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectInVersionOrder(int limit, int skip)
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ThenByDescending(b => b.BuildTime)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectFlight(LevelOfFlight flight, int limit, int skip)
      {
         return await _buildCollection.Find(b => b.FlightLevel == flight)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectLab(string lab, int skip, int limit)
      {
         string labUrl = lab.Replace('/', '-').ToLower();
         return await _buildCollection.Find(b => b.Lab != null && b.LabUrl == labUrl)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectLabCount(string lab)
      {
         return await _buildCollection.Find(b => b.Lab != null && (b.Lab.ToLower() == lab.ToLower()))
            .CountAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectSource(TypeOfSource source, int skip, int limit)
      {
         return await _buildCollection.Find(b => b.SourceType == source)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectSourceCount(TypeOfSource source)
      {
         return await _buildCollection.Find(b => b.SourceType == source)
            .CountAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectYear(int year, int skip, int limit)
      {
         return await _buildCollection.Find(b => b.BuildTime != null &&
         (b.BuildTime > new DateTime(year, 1, 1, 0, 0, 0)) &&
         (b.BuildTime < new DateTime(year, 12, 31, 23, 59, 59)))
            .SortByDescending(b => b.BuildTime)
            .ThenByDescending(b => b.MajorVersion)
            .ThenByDescending(b => b.MinorVersion)
            .ThenByDescending(b => b.Number)
            .ThenByDescending(b => b.Revision)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectYearCount(int year)
      {
         return await _buildCollection.Find(b => b.BuildTime != null &&
         (b.BuildTime > new DateTime(year, 1, 1, 0, 0, 0)) &&
         (b.BuildTime < new DateTime(year, 12, 31, 23, 59, 59)))
            .CountAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectVersion(int major, int minor, int skip, int limit)
      {
         return await _buildCollection.Find(b => b.MajorVersion == major && b.MinorVersion == minor)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectVersionCount(int major, int minor)
      {
         return await _buildCollection.Find(b => b.MajorVersion == major && b.MinorVersion == minor)
            .CountAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildVersion>> SelectBuildVersions()
      {
         var result = await _buildCollection.Aggregate()
            .Group(b => new BuildVersion()
            {
               Major = b.MajorVersion,
               Minor = b.MinorVersion,
            },
            // incoming bullshit hack
            bg => new Tuple<BuildVersion>(bg.Key))
            .SortByDescending(b => b.Item1.Major)
            .ThenByDescending(b => b.Item1.Minor)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<int>> SelectBuildYears()
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.BuildTime != null)
            .Group(b => ((DateTime)b.BuildTime).Year,
            // incoming bullshit hack
            bg => new Tuple<int>(bg.Key))
            .SortByDescending(b => b.Item1)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<string>> SearchBuildLabs(string query)
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Match(b => b.Lab.ToLower().Contains(query.ToLower()))
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<string>> SelectBuildLabs()
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .SortBy(b => b.Item1)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<string>> SelectBuildLabs(byte major, byte minor)
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.MajorVersion == major)
            .Match(b => b.MinorVersion == minor)
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .SortBy(b => b.Item1)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Insert, true)]
      public async Task Insert(BuildModel item)
      {
         item.Id = Guid.NewGuid();
         item.LabUrl = item.GenerateLabUrl();
         await _buildCollection.InsertOneAsync(item);
      }

      [DataObjectMethod(DataObjectMethodType.Insert, false)]
      public async Task InsertAll(IEnumerable<BuildModel> items)
      {
         foreach(var item in items)
         {
            item.Id = Guid.NewGuid();
            item.LabUrl = item.GenerateLabUrl();
         }

         await _buildCollection.InsertManyAsync(items);
      }

      [DataObjectMethod(DataObjectMethodType.Update, true)]
      public async Task Update(BuildModel item)
      {
         BuildModel old = await SelectById(item.Id);
         item.Added = old.Added;
         item.Modified = DateTime.Now;
         item.LabUrl = item.GenerateLabUrl();

         await _buildCollection.ReplaceOneAsync(f => f.Id == item.Id, item);
      }

      [DataObjectMethod(DataObjectMethodType.Delete, true)]
      public async Task DeleteById(Guid id)
      {
         await _buildCollection.DeleteOneAsync(f => f.Id == id);
      }
   }

   public enum TypeOfSource
   {
      [Display(ResourceType = typeof(Model), Name = "PublicRelease")]
      PublicRelease = 0,

      [Display(ResourceType = typeof(Model), Name = "InternalLeak")]
      InternalLeak = 1,

      [Display(ResourceType = typeof(Model), Name = "UpdateGDR")]
      UpdateGDR = 2,

      [Display(ResourceType = typeof(Model), Name = "UpdateLDR")]
      UpdateLDR = 3,

      [Display(ResourceType = typeof(Model), Name = "AppPackage")]
      AppPackage = 4,

      [Display(ResourceType = typeof(Model), Name = "BuildTools")]
      BuildTools = 5,

      [Display(ResourceType = typeof(Model), Name = "Documentation")]
      Documentation = 6,

      [Display(ResourceType = typeof(Model), Name = "Logging")]
      Logging = 7,

      [Display(ResourceType = typeof(Model), Name = "PrivateLeak")]
      PrivateLeak = 8
   }

   public enum LevelOfFlight
   {
      [Display(ResourceType = typeof(Model), Name = "FlightNone")]
      None = 0,

      [Display(ResourceType = typeof(Model), Name = "FlightWIS")]
      WIS = 1,

      [Display(ResourceType = typeof(Model), Name = "FlightWIF")]
      WIF = 2,

      [Display(ResourceType = typeof(Model), Name = "FlightOSG")]
      OSG = 3,

      [Display(ResourceType = typeof(Model), Name = "FlightMSIT")]
      MSIT = 4,

      [Display(ResourceType = typeof(Model), Name = "FlightCanary")]
      Canary = 5
   }

   public class BuildVersion
   {
      public byte Major { get; set; }
      public byte Minor { get; set; }

      public BuildVersion()
      {
      }

      public BuildVersion(byte major, byte minor)
      {
         Major = major;
         Minor = minor;
      }

      public override string ToString()
      {
         return $"{Major}.{Minor}";
      }
   }

   public class BuildGroup
   {
      public byte Major { get; set; }
      public byte Minor { get; set; }
      public ushort Build { get; set; }
      public ushort? Revision { get; set; }

      public override string ToString()
      {
         return Revision.HasValue ?
                    $"{Major}.{Minor}.{Build}.{Revision.Value}" :
                    $"{Major}.{Minor}.{Build}";
      }
   }
}