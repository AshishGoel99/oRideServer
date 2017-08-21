using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace oServer.DbModels
{
    public class Ride : Base
    {
        [BsonElement("OwnerId")]
        public string OwnerId { get; set; }
        [BsonElement("StartTime")]
        public DateTime StartTime { get; set; }
        [BsonElement("Duration")]
        public short Duration { get; set; }
        [BsonElement("FromId")]
        public string FromId { get; set; }
        [BsonElement("ToId")]
        public string ToId { get; set; }
        [BsonElement("Note")]
        public string Note { get; set; }
        [BsonElement("Vehicle")]
        public string Vehicle { get; set; }
        [BsonElement("Fare")]
        public short Fare { get; set; }
    }
}

namespace oServer.UserModels
{
    public class Ride
    {
        public string Owner { get; set; }
        public string StartTime { get; set; }
        public short Duration { get; set; }
        public Location From { get; set; }
        public Location To { get; set; }
        public string Note { get; set; }
        public string Vehicle { get; set; }
        public short Fare { get; set; }
    }
}