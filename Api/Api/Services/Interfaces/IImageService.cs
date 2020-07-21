﻿using Api.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IImageService
    {
        Task<Guid> SaveImage(IFormFile file, string validExtension, string userId);
        bool ProcessImage(Guid guid, string userId, ProcessMode mode);
        FileInfo GetImageFileInfo(Guid guid, string userId, GraphFormat format);
    }
}
