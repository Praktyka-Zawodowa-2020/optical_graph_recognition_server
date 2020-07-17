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
        ///     Uploads graph to user's Google Drive.
        /// </summary>
        /// <remarks>
        ///     Uploads graph to user's Google Drive based on GUID acquired earlier.
        /// </remarks>
        /// <param name="guid">GUID of the graph from previous requests.</param> 
        /// <param name="name">Desired name of uploaded graph. Defaults to "graph".</param> 
        /// <param name="format">Format, in which processed graph is uploaded. Defaults to raw image.</param> 
        /// <response code="400">Bad request.</response>
        [HttpPost("upload/{guid}")]
        public IActionResult SendFileToDrive(Guid guid, [FromQuery] string name = "graph", [FromQuery] GraphFormat format = GraphFormat.Raw)
        {
            var userId = User.Claims.ToList()[0].Value;
            var user = _dataContext.Users.ToList().SingleOrDefault(u => u.Id.ToString() == userId);

            var res = _driveService.CreateFile(user, guid, name, format);
            if (res)
                return Ok(new { message = "succes" });
            else
                return Ok(new { message = "failed" });

        }

        /// <summary>
        ///     Lists files from user's Google Drive.
        /// </summary>
        /// <remarks>
        ///     TEST PURPOSE.
        /// </remarks>
        [HttpGet("getAllFiles")]
        public IActionResult GetFiles()
        {
            var userId = User.Claims.ToList()[0].Value;
            var user = _dataContext.Users.ToList().SingleOrDefault(u => u.Id.ToString() == userId);

            var files = _driveService.GetAllFiles(user);

            return Ok(files);
        }
    }
}