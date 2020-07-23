using Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Api.DTOs
{
    /// <summary>
    /// GraphEntity is an abstract object used to operate on graphs in this API. Each graph entity represents a graph as all files (and differentiates them as defined in GraphFormat) that allow to perform various operations, tracked by GUID in the server's storage. 
    /// <para>It specifies graphs name, owner, creation time and its publicity.</para>
    /// <para>Each GraphEntity in the storage consist of one file for each GraphFile type.</para>
    /// <para>At the beggining, GraphEntity contains only image file. To produce other files, an image file needs to be processed.</para>
    /// </summary>
    public class GraphEntityDTO
    {
        public string Name { get; set; }
        public Guid GUID { get; set; }
        public string OwnersMail { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; } = false;
    }
}
