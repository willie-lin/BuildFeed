using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;
using System.Web.Mvc;
using BuildFeed.Local;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        public IEnumerable<BuildModel> Select()
        {
            var task = _buildCollection.Find(new BsonDocument()).ToListAsync();
            task.Wait();
            return task.Result;
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
        public IEnumerable<BuildModel> SelectInBuildOrder()
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
        public IEnumerable<BuildModel> SelectInVersionOrder()
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
        public IEnumerable<BuildVersion> SelectBuildVersions()
        {
            var task = _buildCollection.DistinctAsync(b => new BuildVersion() { Major = b.MajorVersion, Minor = b.MinorVersion }, b => true);
            task.Wait();
            var outTask = task.Result.ToListAsync();
            outTask.Wait();
            return outTask.Result
                .OrderByDescending(y => y.Major)
                .ThenByDescending(y => y.Minor);
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<int> SelectBuildYears()
        {
            var task = _buildCollection.DistinctAsync(b => b.BuildTime.Value.Year, b => b.BuildTime.HasValue);
            task.Wait();
            var outTask = task.Result.ToListAsync();
            outTask.Wait();
            return outTask.Result.OrderBy(b => b);
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<string> SelectBuildLabs()
        {
            var task = _buildCollection.DistinctAsync(b => b.Lab.ToLower(), b => !string.IsNullOrWhiteSpace(b.Lab));
            task.Wait();
            var outTask = task.Result.ToListAsync();
            outTask.Wait();
            return outTask.Result.OrderBy(b => b);
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<string> SelectBuildLabs(byte major, byte minor)
        {
            var task = _buildCollection.DistinctAsync(b => b.Lab.ToLower(), b => !string.IsNullOrWhiteSpace(b.Lab) && b.MajorVersion == major && b.MinorVersion == minor);
            task.Wait();
            var outTask = task.Result.ToListAsync();
            outTask.Wait();
            return outTask.Result.OrderBy(b => b);
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public void Insert(BuildModel item)
        {
            item.Id = Guid.NewGuid();
            var task = _buildCollection.InsertOneAsync(item);
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

        [DataObjectMethod(DataObjectMethodType.Insert, false)]
        public void InsertAll(IEnumerable<BuildModel> items)
        {
            var task = _buildCollection.InsertManyAsync(items);
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
        PublicRelease,

        [Display(ResourceType = typeof(Model), Name = "InternalLeak")]
        InternalLeak,

        [Display(ResourceType = typeof(Model), Name = "UpdateGDR")]
        UpdateGDR,

        [Display(ResourceType = typeof(Model), Name = "UpdateLDR")]
        UpdateLDR,

        [Display(ResourceType = typeof(Model), Name = "AppPackage")]
        AppPackage,

        [Display(ResourceType = typeof(Model), Name = "BuildTools")]
        BuildTools,

        [Display(ResourceType = typeof(Model), Name = "Documentation")]
        Documentation,

        [Display(ResourceType = typeof(Model), Name = "Logging")]
        Logging,

        [Display(ResourceType = typeof(Model), Name = "PrivateLeak")]
        PrivateLeak
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

    public struct BuildVersion
    {
        public byte Major { get; set; }
        public byte Minor { get; set; }

        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }
    }

    public struct BuildGroup
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