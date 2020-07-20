using System;
using System.IO;
using System.Threading.Tasks;
using Api.Helpers;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        public ImageService(IOptions<AppSettings> appSettings, ILogger<ImageService> logger)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _targetFilePath = _appSettings.StoragePaths.UploadsDirectory;
        }

        private readonly string _targetInFileName = "raw";
        private readonly string _targetFilePath;

        public FileInfo GetImageFileInfo(Guid guid, string userId, GraphFormat format)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_targetFilePath, userId, guid.ToString()));
            if (!dir.Exists)
                return null;

            FileInfo[] files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);

            FileInfo file = null;
            foreach (var item in files)
            {
                if (format == GraphFormat.Raw && Path.GetFileNameWithoutExtension(item.Name).Equals(_targetInFileName))
                    file = item;
                else
                if (format == GraphFormat.GraphML && item.Extension == (".graphml"))
                    file = item;
                else
                if (format == GraphFormat.GraphML && item.Extension == (".g6"))
                    file = item;
            }
            return file;
        }

        public bool ProcessImage(Guid guid, string userId)
        {

            var image = GetImageFileInfo(guid, userId, GraphFormat.Raw);
            var script = _appSettings.StoragePaths.ScriptFullPath;
            var param = "-p " + image.FullName;
            var result = new PythonRunner().Run(script, param);
            _logger.LogInformation("Processing image result:", result);
            return true;
        }

        public async Task<Guid> SaveImage(IFormFile file, string userId)
        {
            Guid guid = Guid.NewGuid();
            var path = Path.Combine(_targetFilePath, userId, guid.ToString());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetInFileName, ext)), FileMode.Create))
                await file.CopyToAsync(targetStream);

            return guid;
        }
    }
}
