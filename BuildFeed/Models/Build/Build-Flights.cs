using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public Task<LevelOfFlight[]> SelectAllFlights(int limit = -1, int skip = 0)
      {
         return Task.Run(() => Enum.GetValues(typeof(LevelOfFlight)) as LevelOfFlight[]);
      }

      public Task<long> SelectAllFlightsCount()
      {
         return Task.Run(() => Enum.GetValues(typeof(LevelOfFlight))
                                   .LongLength);
      }

      public async Task<List<BuildModel>> SelectFlight(LevelOfFlight flight, int limit = -1, int skip = 0)
      {
         var query = _buildCollection.Find(new BsonDocument(nameof(BuildModel.FlightLevel), flight))
                                     .Sort(sortByOrder)
                                     .Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      public async Task<long> SelectFlightCount(LevelOfFlight flight) { return await _buildCollection.CountAsync(new BsonDocument(nameof(BuildModel.FlightLevel), flight)); }
   }
}