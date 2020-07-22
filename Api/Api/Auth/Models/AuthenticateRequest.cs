using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class AuthenticateRequest
    {
        /// <summary>
        /// Token available in the response from Google APIs request. Identity scope must be included in such request in order to obtain this token.
        /// </summary>
        [Required]
        public string IdToken { get; set; }
        /// <summary>
        /// Auth Code acquired by requesting Offline Access from Google APIs.
        /// </summary>
        [Required]
        public string AuthCode { get; set; }
    }
}
