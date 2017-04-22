using MongoDB.Bson.Serialization.Attributes;

namespace BuildFeed.Model.Api
{
    public class FamilyOverview
    {
        [BsonElement("_id")]
        public int Family { get; set; }

        public ulong Count { get; set; }

        public BuildDetails Latest { get; set; }
    }
}