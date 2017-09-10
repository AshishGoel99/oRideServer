// using MongoDB.Bson;
// using MongoDB.Bson.Serialization.Attributes;

namespace oServer.DbModels
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}