using Api.Helpers;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var userCredentialsResult = _googleAuthHandler.GetUserCredentials(userGoogleId);
            var userCredentials = userCredentialsResult == null ? null : userCredentialsResult.Result;

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredentials,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Fields = "*";
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
