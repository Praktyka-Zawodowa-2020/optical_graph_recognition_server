using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
