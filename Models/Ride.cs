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
        public short Fare { get; set; }
        public string ContactNo { get; set; }
        public string Bounds { get; set; }
        public string[] Waypoints { get; set; }
        public List<List<decimal>> PolyLine { get; set; }
        public List<List<List<decimal>>> PolyGon { get; set; }
    }
}