using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;

namespace BuildFeed.Code
{
    public class LongIdGenerator : IIdGenerator
    {
        private const string _idCollectionName = "IDs";
        private MongoClient _dbClient;
        private IMongoCollection<long> _idCollection;

        public LongIdGenerator()
        {
            _dbClient = new MongoClient(new MongoClientSettings()
            {
                Server = new MongoServerAddress(MongoConfig.Host, MongoConfig.Port)
            });

            _idCollection = _dbClient.GetDatabase(MongoConfig.Database).GetCollection<long>(_idCollectionName);
        }

        public object GenerateId(object container, object document)
        {
            var query = Query.EQ("_id", (container).Name);

            return (_idCollection.FindOneAndUpdateAsync(new FindAndModifyArgs()
            {
                Query = query,
                Update = CreateUpdateBuilder(),
                VersionReturned = FindAndModifyDocumentVersion.Modified,
                Upsert = true
            }).AsInt64.ModifiedDocument["seq"]);
        }

        public bool IsEmpty(object id)
        {
            return ((long)id) == 0;
        }
    }
}