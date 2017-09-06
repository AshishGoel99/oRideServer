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

        public string AccessToken { get; set; }
    }
}