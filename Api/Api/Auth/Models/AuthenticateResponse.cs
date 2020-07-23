using Api.Entities;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class AuthenticateResponse
    {
        [JsonIgnore]
        public int Id { get; set; }
        /// <summary>
        /// User's mail.
        /// </summary>
        public string Mail { get; set; }
        /// <summary>
        /// JWT Token used to authorize this API. Is valid for 15 minutes. Until expired, can be send in a
        /// <br>"Authorization: Bearer {token}" header</br>
        /// in order to grant access to the API for user.
        /// </summary>
        public string JwtToken { get; set; }
        /// <summary>
        /// Refresh token used to gain new JWT token if one expires. Valid for 1 week.
        /// </summary>
        public string RefreshToken { get; set; }

        public AuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            Mail = user.Mail;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}
