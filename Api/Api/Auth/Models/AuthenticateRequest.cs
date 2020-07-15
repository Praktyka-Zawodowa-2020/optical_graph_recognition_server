using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public string IdToken { get; set; }
        [Required]
        public string AuthCode { get; set; }
    }
}
