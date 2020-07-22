using Api.Entities;

namespace Api.Models
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string Mail { get; set; }
        public string JwtToken { get; set; }
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
