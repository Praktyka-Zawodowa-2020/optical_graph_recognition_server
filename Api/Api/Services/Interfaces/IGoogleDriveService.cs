using Api.Entities;
using Api.Models;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using User = Api.Entities.User;

namespace Api.Services
{
    public interface IGoogleDriveService
    {
        IList<File> GetAllFiles(User user);
        bool CreateFile(User user, Guid guid, GraphFormat fileFormat);
    }
}
