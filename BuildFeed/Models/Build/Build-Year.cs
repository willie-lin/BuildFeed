using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public async Task<int[]> SelectAllYears(int limit = -1, int skip = 0)
      {
         IAggregateFluent<BsonDocument> query =
            _buildCollection.Aggregate()
               .Match(Builders<BuildModel>.Filter.Ne(b => b.BuildTime, null))
               .Group(new BsonDocument("_id", new BsonDocument("$year", $"${nameof(BuildModel.BuildTime)}")))
               .Sort(new BsonDocument("_id", -1))
               .Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         List<BsonDocument> grouping = await query.ToListAsync();

         return (from g in grouping
                 where !g["_id"].IsBsonNull
                 select g["_id"].AsInt32).ToArray();
      }

      public async Task<long> SelectAllYearsCount()
      {
         List<BsonDocument> query = await _buildCollection.Aggregate().Match(Builders<BuildModel>.Filter.Ne(b => b.BuildTime, null)).Group(new BsonDocument("_id", new BsonDocument("$year", $"${nameof(BuildModel.BuildTime)}"))).ToListAsync();

         return query.Count;
      }

      public async Task<List<BuildModel>> SelectYear(int year, int limit = -1, int skip = 0)
      {
         IFindFluent<BuildModel, BuildModel> query =
            _buildCollection.Find(Builders<BuildModel>.Filter.And(Builders<BuildModel>.Filter.Gte(b => b.BuildTime, new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
               Builders<BuildModel>.Filter.Lte(b => b.BuildTime, new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc)))).Sort(sortByCompileDate).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      public async Task<long> SelectYearCount(int year)
         =>
            await
               _buildCollection.CountAsync(Builders<BuildModel>.Filter.And(Builders<BuildModel>.Filter.Gte(b => b.BuildTime, new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                  Builders<BuildModel>.Filter.Lte(b => b.BuildTime, new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc))));
   }
}