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
      public List<BuildModel> Select()
      {
         var task = _buildCollection.Find(new BsonDocument()).ToListAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<FrontBuildGroup> SelectBuildGroups(int limit, int skip)
      {
         var pipeline = _buildCollection.Aggregate()
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
            .Limit(limit);

         var task = pipeline.ToListAsync();
         task.Wait();

         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public int SelectBuildGroupsCount()
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

         var task = pipeline.ToListAsync();
         task.Wait();

         return task.Result.Count();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public Tuple<BuildGroup, IEnumerable<BuildModel>> SelectSingleBuildGroup(BuildGroup bGroup)
      {
         var pipeline = _buildCollection.Aggregate()
            .Match(b => b.MajorVersion == bGroup.Major)
            .Match(b => b.MinorVersion == bGroup.Minor)
            .Match(b => b.Number == bGroup.Build)
            .Match(b => b.Revision == bGroup.Revision)
            .SortByDescending(b => b.BuildTime);

         var task = pipeline.ToListAsync();
         task.Wait();

         return new Tuple<BuildGroup, IEnumerable<BuildModel>>(bGroup, task.Result);
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public BuildModel SelectById(Guid id)
      {
         var task = _buildCollection.Find(f => f.Id == id).SingleOrDefaultAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public BuildModel SelectByLegacyId(long id)
      {
         var task = _buildCollection.Find(f => f.LegacyId == id).SingleOrDefaultAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<BuildModel> SelectInBuildOrder()
      {
         var task = _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ToListAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<BuildModel> SelectInVersionOrder()
      {
         var task = _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ThenByDescending(b => b.BuildTime)
             .ToListAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<BuildModel> SelectLab(string lab, int skip, int limit)
      {
         var task = _buildCollection.Find(b => b.Lab != null && (b.Lab.ToLower() == lab.ToLower()))
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public long SelectLabCount(string lab)
      {
         var task = _buildCollection.Find(b => b.Lab != null && (b.Lab.ToLower() == lab.ToLower()))
            .CountAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<BuildModel> SelectSource(TypeOfSource source, int skip, int limit)
      {
         var task = _buildCollection.Find(b => b.SourceType == source)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public long SelectSourceCount(TypeOfSource source)
      {
         var task = _buildCollection.Find(b => b.SourceType == source)
            .CountAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<BuildModel> SelectYear(int year, int skip, int limit)
      {
         var task = _buildCollection.Find(b => b.BuildTime.HasValue && b.BuildTime.Value.Year == year)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public long SelectYearCount(int year)
      {
         var task = _buildCollection.Find(b => b.BuildTime.HasValue && b.BuildTime.Value.Year == year)
            .CountAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<BuildModel> SelectVersion(int major, int minor, int skip, int limit)
      {
         var task = _buildCollection.Find(b => b.MajorVersion == major && b.MinorVersion == minor)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public long SelectVersionCount(int major, int minor)
      {
         var task = _buildCollection.Find(b => b.MajorVersion == major && b.MinorVersion == minor)
            .CountAsync();
         task.Wait();
         return task.Result;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<BuildVersion> SelectBuildVersions()
      {
         var task = _buildCollection.Aggregate()
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

         task.Wait();

         // work ourselves out of aforementioned bullshit hack
         return task.Result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public IEnumerable<int> SelectBuildYears()
      {
         var task = _buildCollection.Aggregate()
            .Match(b => b.BuildTime != null)
            .Group(b => ((DateTime)b.BuildTime).Year,
            // incoming bullshit hack
            bg => new Tuple<int>(bg.Key))
            .SortByDescending(b => b.Item1)
            .ToListAsync();

         task.Wait();

         // work ourselves out of aforementioned bullshit hack
         return task.Result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public List<string> SearchBuildLabs(string query)
      {
         var task = _buildCollection.Aggregate()
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Match(b => b.Lab.ToLower().Contains(query.ToLower()))
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .ToListAsync();

         task.Wait();

         // work ourselves out of aforementioned bullshit hack
         return task.Result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public IEnumerable<string> SelectBuildLabs()
      {
         var task = _buildCollection.Aggregate()
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .SortBy(b => b.Item1)
            .ToListAsync();

         task.Wait();

         // work ourselves out of aforementioned bullshit hack
         return task.Result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public IEnumerable<string> SelectBuildLabs(byte major, byte minor)
      {
         var task = _buildCollection.Aggregate()
            .Match(b => b.MajorVersion == major)
            .Match(b => b.MinorVersion == minor)
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .SortBy(b => b.Item1)
            .ToListAsync();

         task.Wait();

         // work ourselves out of aforementioned bullshit hack
         return task.Result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Insert, true)]
      public void Insert(BuildModel item)
      {
         item.Id = Guid.NewGuid();
         var task = _buildCollection.InsertOneAsync(item);
         task.Wait();
      }

      [DataObjectMethod(DataObjectMethodType.Insert, false)]
      public void InsertAll(IEnumerable<BuildModel> items)
      {
         var task = _buildCollection.InsertManyAsync(items);
         task.Wait();
      }

      [DataObjectMethod(DataObjectMethodType.Update, true)]
      public void Update(BuildModel item)
      {
         BuildModel old = SelectById(item.Id);
         item.Added = old.Added;
         item.Modified = DateTime.Now;

         var task = _buildCollection.ReplaceOneAsync(f => f.Id == item.Id, item);
         task.Wait();
      }

      [DataObjectMethod(DataObjectMethodType.Delete, true)]
      public void DeleteById(Guid id)
      {
         var task = _buildCollection.DeleteOneAsync(f => f.Id == id);
         task.Wait();
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

      [Display(ResourceType = typeof(Model), Name = "FlightLow")]
      Low = 1,

      [Display(ResourceType = typeof(Model), Name = "FlightMedium")]
      Medium = 2,

      [Display(ResourceType = typeof(Model), Name = "FlightHigh")]
      High = 3
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