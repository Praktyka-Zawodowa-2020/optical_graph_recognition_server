using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CreateGraphRequest
    {
        /// <summary>
        /// File must be an image. Supported formats are: jpeg, jpg, png and bmp.
        /// </summary>
        [Required]
        public IFormFile File { get; set; }
        /// <summary>
        /// Name of the newly created graph entity. If given none - defaults to "graph".
        /// </summary>
        public string Name { get; set; } = "graph";
    }
}
