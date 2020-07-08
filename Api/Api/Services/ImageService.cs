using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Api.Services
{
    public class ImageService : IImageService
    {
        public Task<Stream> GetImage(Guid guid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ProcessImage(Guid guid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveImage(IFormFile image)
        {
            throw new NotImplementedException();
        }
    }
}
