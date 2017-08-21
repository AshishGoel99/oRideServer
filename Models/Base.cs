using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
 
namespace oServer.DbModels
{
    public class Base
    {
        public ObjectId Id { get; set; }
    }
}