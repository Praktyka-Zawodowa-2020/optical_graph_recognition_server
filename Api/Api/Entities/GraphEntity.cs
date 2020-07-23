using Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Api.Models
{
    /// <summary>
    /// Entity used to operate on graphs in this API. Each graph entity represents a graph as all files (and differentiates them as defined in GraphFormat) that allow to perform various operations, tracked by GUID in the server's storage. 
    /// <para>It specifies graphs name, owner, creation time and its publicity.</para>
    /// <para>Each GraphEntity in the storage consist of one file for each GraphFile type.</para>
    /// <para>At the beggining, GraphEntity contains only image file. To produce other files, an image file needs to be processed.</para>
    /// </summary>
    public class GraphEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid GUID { get; set; }
        public User Owner { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Today;
        public bool IsPublic { get; set; } = false;
    }
}
