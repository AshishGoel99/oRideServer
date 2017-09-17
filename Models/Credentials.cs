using System.ComponentModel.DataAnnotations;

namespace oServer.UserModels
{
    public class Credentials
    {

        [Required]
        public string UserName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string Picture { get; set; }
        [Required]
        public string FbId { get; set; }
        [Required]
        public string FbToken { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}