using System;
using System.Collections.Generic;
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
        [BsonElement("ReturnTime")]
        public DateTime ReturnTime { get; set; }
        [BsonElement("ScheduleType")]
        public short ScheduleType { get; set; }
        // [BsonElement("Duration")]
        // public short Duration { get; set; }
        [BsonElement("Days")]
        public List<short> Days { get; set; }
        [BsonElement("Date")]
        public DateTime Date { get; set; }

        [BsonElement("FromId")]
        public string FromId { get; set; }
        [BsonElement("ToId")]
        public string ToId { get; set; }
        [BsonElement("Note")]
        public string Note { get; set; }
        [BsonElement("SeatsAvail")]
        public short SeatsAvail { get; set; }
        [BsonElement("Vehicle")]
        public string Vehicle { get; set; }
        [BsonElement("Fare")]
        public short Fare { get; set; }
        [BsonElement("ContactNo")]
        public string ContactNo { get; set; }
        [BsonElement("Polyline")]
        public List<List<decimal>> PolyLine { get; set; }
        [BsonElement("Polygon")]
        public List<List<List<decimal>>> PolyGon { get; set; }
    }
}

namespace oServer.UserModels
{
    public class Ride
    {
        public string Owner { get; set; }
        public string StartTime { get; set; }

        public string ReturnTime { get; set; }
        public short ScheduleType { get; set; }
        public List<short> Days { get; set; }
        public string Date { get; set; }
        // public short Duration { get; set; }
        public Location From { get; set; }
        public Location To { get; set; }
        public short SeatsAvail { get; set; }
        public string Note { get; set; }
        public string Vehicle { get; set; }
        public short Fare { get; set; }
        public string ContactNo { get; set; }
        public string Bounds { get; set; }
        public string[] Waypoints { get; set; }
        public List<List<decimal>> PolyLine { get; set; }
        public List<List<List<decimal>>> PolyGon { get; set; }
    }
}