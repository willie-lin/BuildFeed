using BuildFeed;
using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using NServiceKit.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Models
{
    [DataObject]
    public class LabMeta : IHasId<string>
    {
        [Key]
        [Index]
        [@Required]
        public string Id { get; set; }

        [DisplayName("Page Content")]
        [AllowHtml]
        public string PageContent { get; set; }

        [DisplayName("Meta Description")]
        public string MetaDescription { get; set; }



        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public static IEnumerable<LabMeta> Select()
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<LabMeta>();
                return client.GetAll();
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static LabMeta SelectById(string id)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<LabMeta>();
                return client.GetById(id);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public static IEnumerable<string> SelectUnusedLabs()
        {

            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<LabMeta>();
                var labs = Build.SelectBuildLabs();

                var usedLabs = client.GetAll();

                return from l in labs
                       where !usedLabs.Any(ul => ul.Id == l)
                       select l;
            }
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public static void Insert(LabMeta item)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<LabMeta>();
                client.Store(item);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public static void Update(LabMeta item)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<LabMeta>();
                client.Store(item);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Insert, false)]
        public static void InsertAll(IEnumerable<LabMeta> items)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<LabMeta>();
                client.StoreAll(items);
            }
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public static void DeleteById(long id)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<LabMeta>();
                client.DeleteById(id);
            }
        }
    }
}