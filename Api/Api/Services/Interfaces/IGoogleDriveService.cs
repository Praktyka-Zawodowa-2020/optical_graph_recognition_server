using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IGoogleDriveService
    {
        IList<Google.Apis.Drive.v3.Data.File> GetFiles(string userGoogleId);
    }
}
