using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Mail { get; set; }

        [JsonIgnore]
        public string GoogleAccessToken { get; set; }

        [JsonIgnore]
        public string GoogleRefreshToken { get; set; }

        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
    }
}
