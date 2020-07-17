using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Entities;
using Api.Helpers;
using Api.Models;
using Microsoft.AspNetCore.Http;

namespace Api.Services
{
    public class ImageService : IImageService
    {
        private readonly string _targetFilePath = "/app/uploads/";
        private readonly string _targetInFileName = "raw";

        public FileInfo GetImageFileInfo(Guid guid, string userId, FileFormat format)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_targetFilePath, userId, guid.ToString()));
            if (!dir.Exists)
                return null;

            FileInfo[] files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);

            FileInfo file = null;
            foreach (var item in files)
            {
                if (format == FileFormat.Raw && Path.GetFileNameWithoutExtension(item.Name).Equals(_targetInFileName))
                    file = item;

                if (format == FileFormat.GraphML && item.Extension == (".graphml"))
                    file = item;
            }
            return file;
        }

        public async Task<bool> ProcessImage(Guid guid, string userId)
        {

            //var res = new RunCmd().Run("your_python_file.py", "params");
            //Console.WriteLine(res);
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
