using System.ComponentModel.DataAnnotations;

namespace oServer.UserModels
{
    public class SearchQuery
    {
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public short? Frame { get; set; }
        [Required]
        public string Time { get; set; }
    }
}