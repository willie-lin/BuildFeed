using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
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
    public class MetaItemModel
    {
        [Key]
        [BsonId]
        [@Required]
        public MetaItemKey Id { get; set; }

        [DisplayName("Page Content")]
        [AllowHtml]
        public string PageContent { get; set; }

        [DisplayName("Meta Description")]
        public string MetaDescription { get; set; }
    }

    public class MetaItem
    {
        private const string _metaCollectionName = "metaitem";

        private MongoClient _dbClient;
        private IMongoCollection<MetaItemModel> _metaCollection;

        public MetaItem()
        {
            _dbClient = new MongoClient(new MongoClientSettings()
            {
                Server = new MongoServerAddress(MongoConfig.Host, MongoConfig.Port)
            });

            _metaCollection = _dbClient.GetDatabase(MongoConfig.Database).GetCollection<MetaItemModel>(_metaCollectionName);
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<MetaItemModel> Select()
        {
            var task = _metaCollection.Find(new BsonDocument()).ToListAsync();
            task.Wait();
            return task.Result;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public IEnumerable<MetaItemModel> SelectByType(MetaType type)
        {
            var task = _metaCollection.Find(f => f.Id.Type == type).ToListAsync();
            task.Wait();
            return task.Result;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public MetaItemModel SelectById(MetaItemKey id)
        {
            var task = _metaCollection.Find(f => f.Id.Type == id.Type && f.Id.Value == id.Value).SingleOrDefaultAsync();
            task.Wait();
            return task.Result;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<string> SelectUnusedLabs()
        {
            var labs = new Build().SelectBuildLabs();

            var usedLabs = _metaCollection.Find(f => f.Id.Type == MetaType.Lab).ToListAsync();
            usedLabs.Wait();

            return from l in labs
                   where usedLabs.Result.All(ul => ul.Id.Value != l)
                   select l;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<string> SelectUnusedVersions()
        {
            var versions = new Build().SelectBuildVersions();

            var usedVersions = _metaCollection.Find(f => f.Id.Type == MetaType.Version).ToListAsync();
            usedVersions.Wait();

            return from v in versions
                   where usedVersions.Result.All(ul => ul.Id.Value != v.ToString())
                   select v.ToString();
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public IEnumerable<string> SelectUnusedYears()
        {
            var years = new Build().SelectBuildYears();

            var usedYears = _metaCollection.Find(f => f.Id.Type == MetaType.Year).ToListAsync();
            usedYears.Wait();

            return from y in years
                   where usedYears.Result.All(ul => ul.Id.Value != y.ToString())
                   select y.ToString();
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public void Insert(MetaItemModel item)
        {
            var task = _metaCollection.InsertOneAsync(item);
            task.Wait();
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public void Update(MetaItemModel item)
        {
            var task = _metaCollection.ReplaceOneAsync(f => f.Id.Type == item.Id.Type && f.Id.Value == item.Id.Value, item);
            task.Wait();
        }

        [DataObjectMethod(DataObjectMethodType.Insert, false)]
        public void InsertAll(IEnumerable<MetaItemModel> items)
        {
            var task = _metaCollection.InsertManyAsync(items);
            task.Wait();
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public void DeleteById(MetaItemKey id)
        {
            var task = _metaCollection.DeleteOneAsync(f => f.Id.Type == id.Type && f.Id.Value == id.Value);
            task.Wait();
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