using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Helpers;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("api/drive/")]
    [ApiController]
    public class GoogleDriveController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IGoogleDriveService _driveService;

        public GoogleDriveController(DataContext dataContext, IGoogleDriveService driveService)
        {
            _dataContext = dataContext;
            _driveService = driveService;
        }

        /// <summary>
        ///     Lists files from user's google drive.
        /// </summary>
        /// <remarks>
        ///     For testing purposes.
        /// </remarks>
        [HttpGet("getAllFiles")]
        public IActionResult GetFiles()
        {
            var userId = User.Claims.ToList()[0].Value;
            var user = _dataContext.Users.ToList().SingleOrDefault(u => u.Id.ToString() == userId);

            var files = _driveService.GetFiles(user.GoogleId);


            return Ok(files);
        }
    }
}