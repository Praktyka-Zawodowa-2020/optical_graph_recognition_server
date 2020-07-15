using Api.Helpers;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private GoogleAuthHandler _googleAuthHandler;
        private DataContext _context;
        public GoogleDriveService(DataContext context, GoogleAuthHandler googleAuthHandler)
        {
            _googleAuthHandler = googleAuthHandler;
            _context = context;
        }
        public IList<Google.Apis.Drive.v3.Data.File> GetFiles(string userGoogleId)
        {
            var user = _context.Users.SingleOrDefault(u => u.GoogleId == userGoogleId);
            var userCredentials = _googleAuthHandler.GetUserCredentials(userGoogleId).Result;
            if (userCredentials == null) return null;

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentials,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Fields = "*/name";
            try
            {
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
                return files;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
