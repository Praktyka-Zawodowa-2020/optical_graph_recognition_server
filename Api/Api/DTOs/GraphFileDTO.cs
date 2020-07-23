using System.IO;

namespace Api.Models
{
    /// <summary>
    /// DTO for graph files.
    /// </summary>
    public class GraphFileDTO
    {
        /// <summary>
        /// Represents an actual file in the physical storage.
        /// </summary>
        public FileInfo File { get; set; }
        /// <summary>
        /// Meta name of the graph entity, the File comes under.
        /// </summary>
        public string Name { get; set; }
    }
}
