namespace oServer.UserModels
{
    public class SearchQuery
    {
        public Location From { get; set; }
        public Location To { get; set; }
        public short TimeFrame { get; set; }
        public string DepartureTime { get; set; }
    }
}