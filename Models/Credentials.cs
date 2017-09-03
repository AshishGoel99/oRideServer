using System.ComponentModel.DataAnnotations;

namespace oServer.UserModels
{
    public class Credentials
    {

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string AccessToken { get; set; }
    }
}