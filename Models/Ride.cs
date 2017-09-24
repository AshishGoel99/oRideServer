using System.Collections.Generic;

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
        public float Fare { get; set; }
        public string ContactNo { get; set; }
        public string Bounds { get; set; }
        public List<string> Waypoints { get; set; }
        public string PolyLine { get; set; }
        public string PolyGon { get; set; }
        public string Id { get; set; }
    }
}