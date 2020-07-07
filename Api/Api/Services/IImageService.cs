using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IImageService
    {
        Task<bool> SaveImage(IFormFile image);
        Task<bool> ProcessImage(Guid guid);
        Task<Stream> GetImage(Guid guid);
    }
}
