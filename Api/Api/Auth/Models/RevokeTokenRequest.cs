using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class RevokeTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
