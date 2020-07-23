using System.IO;

namespace Api.Models
{
    /// <summary>
    /// DTO for graph files.
    /// </summary>
    public class GraphFile
    {
        /// <summary>
        /// Represents actual file in the physical storage.
        /// </summary>
        public FileInfo File { get; set; }
        /// <summary>
        /// Specifies meta name for graph entity.
        /// </summary>
        public string Name { get; set; }
    }
}
