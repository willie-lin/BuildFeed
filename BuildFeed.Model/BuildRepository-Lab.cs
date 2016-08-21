using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Model
{
   public partial class BuildRepository
   {
      public async Task<string[]> SelectAllLabs(int limit = -1, int skip = 0)
      {
         IAggregateFluent<BsonDocument> query = _buildCollection.Aggregate().Group(new BsonDocument("_id", $"${nameof(Build.Lab)}")).Sort(new BsonDocument("_id", 1)).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         List<BsonDocument> grouping = await query.ToListAsync();

         return (from g in grouping
                 where !g["_id"].IsBsonNull
                 select g["_id"].AsString).ToArray();
      }

      public async Task<string[]> SelectLabsForVersion(int major, int minor)
      {
         IAggregateFluent<BsonDocument> query = _buildCollection.Aggregate().Match(new BsonDocument
         {
            new BsonElement(nameof(Build.MajorVersion), major),
            new BsonElement(nameof(Build.MinorVersion), minor)
         }).Group(new BsonDocument("_id", $"${nameof(Build.Lab)}")).Sort(new BsonDocument("_id", 1));

         List<BsonDocument> grouping = await query.ToListAsync();

         return (from g in grouping
                 where !g["_id"].IsBsonNull
                 select g["_id"].AsString).ToArray();
      }

      public async Task<List<string>> SearchLabs(string search)
      {
         List<Tuple<string>> result = await _buildCollection.Aggregate().Match(b => b.Lab != null).Match(b => b.Lab != "").Match(b => b.Lab.ToLower().Contains(search.ToLower())).Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key)).ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      public async Task<long> SelectAllLabsCount()
      {
         IAggregateFluent<BsonDocument> query = _buildCollection.Aggregate().Group(new BsonDocument("_id", new BsonDocument(nameof(Build.Lab), $"${nameof(Build.Lab)}"))).Sort(new BsonDocument("_id", 1));

         List<BsonDocument> grouping = await query.ToListAsync();

         return grouping.Count;
      }

      public async Task<List<Build>> SelectLab(string lab, int limit = -1, int skip = 0)
      {
         IFindFluent<Build, Build> query = _buildCollection.Find(new BsonDocument(nameof(Build.LabUrl), lab)).Sort(sortByCompileDate).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      public async Task<long> SelectLabCount(string lab) => await _buildCollection.CountAsync(new BsonDocument(nameof(Build.LabUrl), lab));
   }
}