using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace oServer.DbModels
{
    public class Location : Base
    {
        [BsonElement("Name")]
        public string Name { get; set; }
        [BsonElement("Latitude")]
        public decimal Latitude { get; set; }
        [BsonElement("Longitude")]
        public decimal Longitude { get; set; }
    }
}

namespace oServer.UserModels
{
    public class Location
    {
        public string Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}