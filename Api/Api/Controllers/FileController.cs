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
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string _targetFilePath = "/Uploads/";
        private readonly string _targetInFileName = "in";
        private readonly string _targetOutFileName = "out";

        private readonly IImageValidator _imageValidator;

        public FileController(IImageValidator imageValidator)
        {
            _imageValidator = imageValidator;
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null)
                return BadRequest(new { status = false, message = $"Please enter image file specyfying content-type as multipart/form-data under the key 'file'." });
            if (file.Length == 0)
                return BadRequest(new { message = $"Empty file provided." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_imageValidator.IsValid(file.OpenReadStream(), ext))
                return BadRequest(new { status = false, message = $"Wrong file format" });


            Guid guid = Guid.NewGuid();
            var path = Path.Combine(_targetFilePath, guid.ToString());
            var trustedFileNameForFileStorage = Path.GetRandomFileName();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetInFileName,ext)), FileMode.Create))
                await file.CopyToAsync(targetStream);

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetOutFileName, ext)), FileMode.Create))
                await file.CopyToAsync(targetStream);

            return Ok(new { status = true, message = $"You've just succesfully uploaded a file \'{file.FileName}\'.", guid });
        }

        [HttpGet("{guid}")]
        public IActionResult Get(Guid guid)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_targetFilePath, guid.ToString()));
            FileInfo[] files = dir.GetFiles(string.Concat(_targetOutFileName, "*"), SearchOption.TopDirectoryOnly);

            string path = string.Empty;
            string name = string.Empty;
            foreach (var item in files)
            {
                path = item.FullName;
                name = item.Name;
            }

            if (path.Equals(string.Empty))
                return NotFound(new { status = false, message = $"File not found." });

            var stream = System.IO.File.OpenRead(path);

            return File(stream, "application/octet-stream", name); 
        }
    }
}