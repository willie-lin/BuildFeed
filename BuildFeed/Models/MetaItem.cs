using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using NServiceKit.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Models
{
    [DataObject]
    public class MetaItem : IHasId<MetaItemKey>
    {
        [Key]
        [Index]
        [@Required]
        public MetaItemKey Id { get; set; }

        [DisplayName("Page Content")]
        [AllowHtml]
        public string PageContent { get; set; }

        [DisplayName("Meta Description")]
        public string MetaDescription { get; set; }



        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<MetaItem> Select()
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                return client.GetAll();
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public static IEnumerable<MetaItem> SelectByType(MetaType type)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                return from t in client.GetAll()
                       where t.Id.Type == type
                       select t;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static MetaItem SelectById(MetaItemKey id)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                return client.GetById(id);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<string> SelectUnusedLabs()
        {

            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                var labs = Build.SelectBuildLabs();

                var usedLabs = from u in client.GetAll()
                               where u.Id.Type == MetaType.Lab
                               select u;

                return from l in labs
                       where usedLabs.All(ul => ul.Id.Value != l)
                       select l;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<string> SelectUnusedVersions()
        {

            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                var versions = Build.SelectBuildVersions();

                var usedLabs = from u in client.GetAll()
                               where u.Id.Type == MetaType.Version
                               select u;

                return from v in versions
                       where usedLabs.All(ul => ul.Id.Value != v.ToString())
                       select v.ToString();
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<string> SelectUnusedYears()
        {

            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                var years = Build.SelectBuildYears();

                var usedYears = from u in client.GetAll()
                               where u.Id.Type == MetaType.Year
                               select u;

                return from y in years
                       where usedYears.All(ul => ul.Id.Value != y.ToString())
                       select y.ToString();
            }
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public static void Insert(MetaItem item)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                client.Store(item);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public static void Update(MetaItem item)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                client.Store(item);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Insert, false)]
        public static void InsertAll(IEnumerable<MetaItem> items)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                client.StoreAll(items);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public static void DeleteById(long id)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<MetaItem>();
                client.DeleteById(id);
            }
        }
    }

    public struct MetaItemKey
    {
        public string Value { get; set; }
        public MetaType Type { get; set; }

        public MetaItemKey(string id)
        {
            var items = id.Split(':');
            Type = (MetaType)Enum.Parse(typeof(MetaType), items[0]);
            Value = items[1];
        }

        public override string ToString()
        {
            return $"{Type}:{Value}";
        }
    }

    public enum MetaType
    {
        Lab,
        Version,
        Source,
        Year
    }
}