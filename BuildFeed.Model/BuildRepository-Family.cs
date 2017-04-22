using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildFeed.Model.Api;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace BuildFeed.Model
{
    public partial class BuildRepository
    {
        public Task<ProjectFamily[]> SelectAllFamilies(int limit = -1, int skip = 0) => Task.Run(() => Enum.GetValues(typeof(ProjectFamily)) as ProjectFamily[]);

        public Task<long> SelectAllFamiliesCount() => Task.Run(() => Enum.GetValues(typeof(ProjectFamily)).LongLength);

        public async Task<List<Build>> SelectFamily(ProjectFamily family, int limit = -1, int skip = 0)
        {
            IFindFluent<Build, Build> query = _buildCollection.Find(new BsonDocument(nameof(Build.Family), family)).Sort(sortByOrder).Skip(skip);

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            return await query.ToListAsync();
        }

        public async Task<long> SelectFamilyCount(ProjectFamily family) => await _buildCollection.CountAsync(new BsonDocument(nameof(Build.Family), family));

        public async Task<List<FamilyOverview>> SelectFamilyOverviews()
        {
            IAggregateFluent<BsonDocument> families = _buildCollection.Aggregate()
                .Sort(sortByOrder)
                .Group(new BsonDocument
                {
                    new BsonElement("_id", $"${nameof(Build.Family)}"),
                    new BsonElement(nameof(FamilyOverview.Count), new BsonDocument("$sum", 1)),
                    new BsonElement(nameof(FamilyOverview.Latest), new BsonDocument("$first", "$$CURRENT"))
                })
                .Sort(new BsonDocument("_id", -1));

            List<BsonDocument> result = await families.ToListAsync();

            return (from o in result
                    select BsonSerializer.Deserialize<FamilyOverview>(o)).ToList();
        }
    }
}