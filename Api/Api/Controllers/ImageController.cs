using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/image")]
    [ApiController]
    public class ImageController : ControllerBase
    {

        private readonly IImageValidator _imageValidator;
        private readonly IImageService _imageService;

        public ImageController(IImageValidator imageValidator, IImageService imageService)
        {
            _imageValidator = imageValidator;
            _imageService = imageService;
        }

        /// <summary>
        ///     Uploads an image file to the server.
        /// </summary>
        /// <remarks>
        ///     Uploads an image file and then creates a graph entity in the server, by processing an image with python script. 
        ///     Returns a Guid, which can be used later to identify each graph source. 
        /// </remarks>
        /// <param name="file">File must be an image. Supported formats are: jpeg, jpg, png and bmp.</param> 
        /// <response code="200">Returns guid to the newly created resource</response>
        /// <response code="400"> Returns error message if the file is not valid</response> 
        [HttpPost("process")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null)
                return BadRequest(new { message = "Please enter image file specyfying content-type as multipart/form-data" });
            if (file.Length == 0)
                return BadRequest(new { message = "Empty file" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_imageValidator.IsValid(file.OpenReadStream(), ext))
                return BadRequest(new { message = "Wrong file format" });

            var userId = User.Claims.ToList()[0].Value;

            var guid = await _imageService.SaveImage(file, userId);

            var result = await _imageService.ProcessImage(guid, userId);

            return Ok(new { guid });
        }
        /// <summary>
        ///     Downloads desired graph.
        /// </summary>
        /// <remarks>
        ///     Returns desired graph based on previous request's GUID.
        /// </remarks>
        /// <param name="guid">GUID to the created graph.</param> 
        /// <param name="name">Desired name of returned graph. Defaults to "graph".</param> 
        /// <param name="format">Format, in which processed graph file is returned. Defaults to raw image.</param> 
        /// <response code="200"> Returns the graph file in response body</response>
        /// <response code="400">Returns error message if the file is not found.</response>
        [HttpGet("get/{guid}")]
        public IActionResult Get(Guid guid, [FromQuery] string name = "graph", [FromQuery] FileFormat format = FileFormat.Raw)
        {
            var userId = User.Claims.ToList()[0].Value;

            var file = _imageService.GetImageFileInfo(guid, userId, format);

            if (file == null)
                return BadRequest(new { message = "There is no such file" });

            var stream = System.IO.File.OpenRead(file.FullName);

            return File(stream, "application/octet-stream", String.Concat(name, file.Extension));
        }
    }
}
