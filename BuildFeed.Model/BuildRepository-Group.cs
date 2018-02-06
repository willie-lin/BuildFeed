using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildFeed.Model.View;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Model
{
    public partial class BuildRepository
    {
        public async Task<FrontBuildGroup[]> SelectAllGroups(int limit = -1, int skip = 0)
        {
            var query = _buildCollection.Aggregate()
                .Group(new BsonDocument
                {
                    new BsonElement("_id",
                        new BsonDocument
                        {
                            new BsonElement(nameof(BuildGroup.Major), $"${nameof(Build.MajorVersion)}"),
                            new BsonElement(nameof(BuildGroup.Minor), $"${nameof(Build.MinorVersion)}"),
                            new BsonElement(nameof(BuildGroup.Build), $"${nameof(Build.Number)}"),
                            new BsonElement(nameof(BuildGroup.Revision), $"${nameof(Build.Revision)}")
                        }),
                    new BsonElement("date", new BsonDocument("$max", $"${nameof(Build.BuildTime)}")),
                    new BsonElement("count", new BsonDocument("$sum", 1))
                })
                .Sort(new BsonDocument
                {
                    new BsonElement($"_id.{nameof(BuildGroup.Major)}", -1),
                    new BsonElement($"_id.{nameof(BuildGroup.Minor)}", -1),
                    new BsonElement($"_id.{nameof(BuildGroup.Build)}", -1),
                    new BsonElement($"_id.{nameof(BuildGroup.Revision)}", -1)
                })
                .Skip(skip);

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            var grouping = await query.ToListAsync();

            return (from g in grouping
                select new FrontBuildGroup
                {
                    Key = new BuildGroup
                    {
                        Major = (uint)g["_id"].AsBsonDocument[nameof(BuildGroup.Major)].AsInt32,
                        Minor = (uint)g["_id"].AsBsonDocument[nameof(BuildGroup.Minor)].AsInt32,
                        Build = (uint)g["_id"].AsBsonDocument[nameof(BuildGroup.Build)].AsInt32,
                        Revision = (uint?)g["_id"].AsBsonDocument[nameof(BuildGroup.Revision)].AsNullableInt32
                    },
                    LastBuild = g["date"].ToNullableUniversalTime(),
                    BuildCount = g["count"].AsInt32
                }).ToArray();
        }

        public async Task<long> SelectAllGroupsCount()
        {
            var grouping = await _buildCollection.Aggregate()
                .Group(new BsonDocument
                {
                    new BsonElement("_id",
                        new BsonDocument
                        {
                            new BsonElement(nameof(BuildGroup.Major), $"${nameof(Build.MajorVersion)}"),
                            new BsonElement(nameof(BuildGroup.Minor), $"${nameof(Build.MinorVersion)}"),
                            new BsonElement(nameof(BuildGroup.Build), $"${nameof(Build.Number)}"),
                            new BsonElement(nameof(BuildGroup.Revision), $"${nameof(Build.Revision)}")
                        })
                })
                .ToListAsync();
            return grouping.Count;
        }

        public async Task<List<Build>> SelectGroup(BuildGroup group, int limit = -1, int skip = 0)
        {
            var query = _buildCollection.Find(new BsonDocument
                {
                    new BsonElement(nameof(Build.MajorVersion), group.Major),
                    new BsonElement(nameof(Build.MinorVersion), group.Minor),
                    new BsonElement(nameof(Build.Number), group.Build),
                    new BsonElement(nameof(Build.Revision), group.Revision)
                })
                .Sort(new BsonDocument
                {
                    new BsonElement(nameof(Build.BuildTime), 1)
                })
                .Skip(skip);

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            return await query.ToListAsync();
        }

        public async Task<long> SelectGroupCount(BuildGroup group) => await _buildCollection.CountAsync(new BsonDocument
        {
            new BsonElement(nameof(Build.MajorVersion), group.Major),
            new BsonElement(nameof(Build.MinorVersion), group.Minor),
            new BsonElement(nameof(Build.Number), group.Build),
            new BsonElement(nameof(Build.Revision), group.Revision)
        });
    }
}