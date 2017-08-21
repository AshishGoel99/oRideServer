using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace oServer.DbModels
{
    public class User : Base
    {
        [BsonElement("UserName")]
        public string Name { get; set; }
    }
}