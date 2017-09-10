using System.ComponentModel.DataAnnotations;

namespace oServer.UserModels
{
    public class SearchQuery
    {
        [Required]
        public Location From { get; set; }
        [Required]
        public Location To { get; set; }
        public short Frame { get; set; }
        public string Time { get; set; }
    }
}