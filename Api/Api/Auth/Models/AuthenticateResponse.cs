using Api.Entities;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string Mail { get; set; }
        public string JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
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
