// using MongoDB.Bson;
// using MongoDB.Bson.Serialization.Attributes;

namespace oServer.DbModels
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string Picture { get; set; }
        public string FbId { get; set; }
        public string Email { get; set; }
        public string PushId { get; set; }
    }
}