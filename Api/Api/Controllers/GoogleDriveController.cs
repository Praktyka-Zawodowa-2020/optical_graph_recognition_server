using System;
using System.Linq;
using Api.Helpers;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
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
        ///     Uploads a graph file to user's Google Drive.
        /// </summary>
        /// <remarks>
        ///     Uploads graph file to user's Google Drive.
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param>
        /// <param name="format">Format, in which the graph file is uploaded. Defaults to raw image.</param> 
        /// <response code="400">Bad request.</response>
        [HttpPost("upload/{guid}")]
        public IActionResult SendFileToDrive(Guid guid, [FromQuery] GraphFormat format = GraphFormat.RawImage)
        {
            var userId = User.Claims.ToList()[0].Value;
            var user = _dataContext.Users.ToList().SingleOrDefault(u => u.Id.ToString() == userId);

            var res = _driveService.CreateFile(user, guid, format);
            if (res)
                return Ok(new { message = "succes" });
            else
                return Ok(new { message = "failed" });

        }
    }
}