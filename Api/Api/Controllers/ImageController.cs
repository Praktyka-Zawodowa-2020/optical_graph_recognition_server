using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Produces("text/plain")]
    [Route("api/image")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly string _targetFilePath = "/Uploads/";
        private readonly string _targetInFileName = "in";
        private readonly string _targetOutFileName = "out";

        private readonly IImageValidator _imageValidator;

        public ImageController(IImageValidator imageValidator)
        {
            _imageValidator = imageValidator;
        }

        /// <summary>
        ///     Uploads an image file to the server.
        /// </summary>
        /// <param name="file">File must be an image. Supported formats are: jpeg, jpg, png and bmp.</param> 
        /// <returns>A guid to newly created source in the server, which will be used for later requests.</returns>
        /// <response code="200">Returns guid to the newly created resource</response>
        /// <response code="400"> Returns error message if the file is not valid</response> 
        [HttpPost("process")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null)
                return BadRequest($"Please enter image file specyfying content-type as multipart/form-data under the key 'file'");
            if (file.Length == 0)
                return BadRequest($"Empty file provided");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_imageValidator.IsValid(file.OpenReadStream(), ext))
                return BadRequest($"Wrong file format");


            Guid guid = Guid.NewGuid();
            var path = Path.Combine(_targetFilePath, guid.ToString());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetInFileName, ext)), FileMode.Create))
                await file.CopyToAsync(targetStream);

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetOutFileName, ext)), FileMode.Create))
                await file.CopyToAsync(targetStream);

            return Ok(guid.ToString());
        }
        /// <summary>
        ///     Downloads desired file.
        /// </summary>
        /// <remarks>
        ///     Returns desired file based on previous requests GUID.
        /// </remarks>
        /// <param name="guid">GUID to the request made previously.</param> 
        /// <response code="200"> Returns the file in response body</response>
        /// <response code="400">Returns error message if the file is not found</response>
        [HttpGet("get/{guid}")]
        public IActionResult Get(Guid guid)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_targetFilePath, guid.ToString()));
            if (!dir.Exists)
                return BadRequest("There is no history request with this guid");

            FileInfo[] files = dir.GetFiles(string.Concat(_targetOutFileName, "*"), SearchOption.TopDirectoryOnly);

            string path = string.Empty;
            string name = string.Empty;
            foreach (var item in files)
            {
                path = item.FullName;
                name = item.Name;
            }

            if (path.Equals(string.Empty))
                return BadRequest($"File not found.");

            var stream = System.IO.File.OpenRead(path);

            return File(stream, "application/octet-stream", name);
        }
    }
}