using Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Api.Models
{
    public class GraphEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid GUID { get; set; }
        [JsonIgnore]
        public User Owner { get; set; }
        public string OwnersMail => Owner!=null ? Owner.Mail : "unknown";
        public DateTime CreatedAt { get; set; } = DateTime.Today;
        public bool IsPublic { get; set; } = false;
    }
}
