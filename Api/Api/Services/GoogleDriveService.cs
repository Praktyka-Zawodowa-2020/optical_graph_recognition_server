using Api.Entities;
using Api.Helpers;
using Api.Models;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using MimeTypes;
using System;
using System.Collections.Generic;
using File = Google.Apis.Drive.v3.Data.File;
using User = Api.Entities.User;

namespace Api.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly GoogleAuthHandler _googleAuthHandler;
        private readonly IGraphService _graphService;
        private readonly DataContext _context;
        public GoogleDriveService(DataContext context, GoogleAuthHandler googleAuthHandler, IGraphService graphService)
        {
            _googleAuthHandler = googleAuthHandler;
            _graphService = graphService;
            _context = context;
        }

        public bool CreateFile(User user, Guid guid, GraphFormat fileFormat)
        {
            var userCredentials = _googleAuthHandler.GetUserCredentials(user.GoogleId).Result;
            if (userCredentials == null) return false;

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentials,
            });

            // Create or find folder for appliation
            var folderId = CreateFolder(user, "Optical Graph Recognition App");

            // Define parameters of request.
            var graphFile = _graphService.GetGraphFile(guid, user.Id, fileFormat);
            if (graphFile == null) return false;
            var uploadStream = new System.IO.FileStream(graphFile.File.FullName,
                                                System.IO.FileMode.Open,
                                                System.IO.FileAccess.Read);
            // Get the media upload request object.
            var fileToCreate = new File
            {
                Name = graphFile.Name,
                Parents = new List<string>() { folderId }
            };

            var mimeType = MimeTypeMap.GetMimeType(graphFile.File.Extension);

            var uploadRequest = driveService.Files.Create(fileToCreate, uploadStream, mimeType);
            uploadRequest.Upload();

            return true;
        }

        public IList<File> GetAllFiles(User user)
        {
            var userCredentials = _googleAuthHandler.GetUserCredentials(user.GoogleId).Result;
            if (userCredentials == null) return null;

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentials,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Fields = "*/name";

            IList<File> files = listRequest.Execute().Files;
            return files;
        }

        // helper methods
        private string CreateFolder(User user, string folderName)
        {
            var userCredentials = _googleAuthHandler.GetUserCredentials(user.GoogleId).Result;
            if (userCredentials == null) return null;

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentials,
            });

            string id = Exists(user, folderName);
            if (id != null)
                return id;

            var file = new File();
            file.Name = folderName;
            file.MimeType = "application/vnd.google-apps.folder";
            var request = driveService.Files.Create(file);
            request.Fields = "id";
            return request.Execute().Id;
        }
        private string Exists(User user, string name)
        {
            var userCredentials = _googleAuthHandler.GetUserCredentials(user.GoogleId).Result;
            if (userCredentials == null) return null;

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentials,
            });

            var listRequest = driveService.Files.List();
            listRequest.PageSize = 100;
            listRequest.Q = $"trashed = false and name contains '{name}' and 'root' in parents";
            listRequest.Fields = "files(name, id)";
            var files = listRequest.Execute().Files;

            foreach (var file in files)
            {
                if (name == file.Name)
                    return file.Id;
            }
            return null;
        }
    }
}
