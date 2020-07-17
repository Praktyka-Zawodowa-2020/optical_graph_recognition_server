using Api.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IImageService
    {
        Task<Guid> SaveImage(IFormFile file, string userId);
        Task<bool> ProcessImage(Guid guid, string userId);
        FileInfo GetImageFileInfo(Guid guid, string userId, GraphFormat format);
    }
}
