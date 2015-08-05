using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;
using System.Web.Mvc;
using BuildFeed.Local;

namespace BuildFeed.Models
{
    [DataObject]
    public class Build
    {
        [Key]
        [AutoIncrement]
        [Index]
        public long Id { get; set; }

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

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public static IEnumerable<Build> Select()
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                return client.GetAll();
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static Build SelectById(long id)
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                return client.GetById(id);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<Build> SelectInBuildOrder()
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                return client.GetAll()
                    .OrderByDescending(b => b.BuildTime)
                    .ThenByDescending(b => b.MajorVersion)
                    .ThenByDescending(b => b.MinorVersion)
                    .ThenByDescending(b => b.Number)
                    .ThenByDescending(b => b.Revision);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<Build> SelectInVersionOrder()
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                return client.GetAll()
                    .OrderByDescending(b => b.MajorVersion)
                    .ThenByDescending(b => b.MinorVersion)
                    .ThenByDescending(b => b.Number)
                    .ThenByDescending(b => b.Revision)
                    .ThenByDescending(b => b.BuildTime);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<BuildVersion> SelectBuildVersions()
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                var results = client.GetAll()
                    .GroupBy(b => new BuildVersion() { Major = b.MajorVersion, Minor = b.MinorVersion })
                    .Select(b => new BuildVersion() { Major = b.First().MajorVersion, Minor = b.First().MinorVersion })
                    .OrderByDescending(y => y.Major)
                    .ThenByDescending(y => y.Minor);
                return results;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<int> SelectBuildYears()
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                var results = client.GetAll().Where(b => b.BuildTime.HasValue)
                    .GroupBy(b => b.BuildTime.Value.Year)
                    .Select(b => b.Key)
                    .OrderByDescending(y => y);
                return results;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<string> SelectBuildLabs()
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                var results = client.GetAll()
                    .Where(b => !string.IsNullOrWhiteSpace(b.Lab))
                    .GroupBy(b => b.Lab.ToLower())
                    .Select(b => b.Key)
                    .OrderBy(s => s);
                return results;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<string> SelectBuildLabs(byte major, byte minor)
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                var results = client.GetAll()
                    .Where(b => !string.IsNullOrWhiteSpace(b.Lab))
                    .Where(b => b.MajorVersion == major)
                    .Where(b => b.MinorVersion == minor)
                    .GroupBy(b => b.Lab.ToLower())
                    .Select(b => b.Key)
                    .OrderBy(s => s);
                return results;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public static void Insert(Build item)
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                item.Id = client.GetNextSequence();
                client.Store(item);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public static void Update(Build item)
        {
            Build old = SelectById(item.Id);
            item.Added = old.Added;
            item.Modified = DateTime.Now;

            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                client.Store(item);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Insert, false)]
        public static void InsertAll(IEnumerable<Build> items)
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                client.StoreAll(items);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public static void DeleteById(long id)
        {
            using (RedisClient rClient = new RedisClient(MongoConfig.Host, MongoConfig.Port, db: MongoConfig.Database))
            {
                var client = rClient.As<Build>();
                client.DeleteById(id);
            }
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