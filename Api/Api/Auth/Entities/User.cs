using Api.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api.Entities
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Mail { get; set; }
        public List<GraphEntity> GraphEntities { get; set; }
        
        [JsonIgnore]
        public string GoogleId{ get; set; }
        
        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }

    }
}
