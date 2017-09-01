using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace oServer.DbModels
{
    public class User : Base
    {
        [BsonElement("UserName")]
        public string Name { get; set; }
        [BsonElement("Email")]
        public string Email { get; set; }
        [BsonElement("Password")]
        public string Password { get; set; }
    }
}