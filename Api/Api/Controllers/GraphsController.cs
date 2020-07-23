﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{   
    // TODO: Async methods
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), 400)]
    [Route("api/graphs")]
    [ApiController]
    public class GraphsController : ControllerBase
    {
        private readonly ImageValidator _imageValidator;
        private readonly IGraphService _imageService;

        public GraphsController(ImageValidator imageValidator, IGraphService imageService)
        {
            _imageValidator = imageValidator;
            _imageService = imageService;
        }

        /// <summary>
        ///     Uploads an image file to the server.
        /// </summary>
        /// <remarks>
        ///     Uploads an image file and creates a graph entity (not processed or anything) in the server's storage. 
        ///     Returns a GUID, which can be used later to identify each graph source. 
        /// </remarks>
        /// <response code="200">Returns a GUID to the newly created resource</response>
        /// <response code="400"> Returns error message if the file is not valid</response> 
        [ProducesResponseType(typeof(Guid), 200)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateGraphRequest model)
        {
            var file = model.File;
            if (file == null)
                return BadRequest(new ErrorMessage("No file provided"));
            if (file.Length == 0)
                return BadRequest(new ErrorMessage("Empty file"));

            string validExtension = _imageValidator.GetValidExtension(file.OpenReadStream());
            if (validExtension.Equals(String.Empty))
                return BadRequest(new ErrorMessage("Wrong file signature"));

            var userId = GetUserId();

            var guid = await _imageService.CreateGraphEntity(model, validExtension, userId);

            if (guid != null)
                return Ok(new { guid });
            else
                return BadRequest(new ErrorMessage("Saving image gone wrong"));
        }

        /// <summary>
        /// Processes an image file of the graph entity.
        /// </summary>
        /// <remarks>
        /// Takes an image file of the specified graph entity, processes it with a python script and returns graph file in a chosen file format.
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param> 
        /// <param name="mode">Mode, in which an image file of the graph entity is processed by the script. Defaults to GRID_BG</param> 
        /// <param name="format">Format, in which the processed graph file is returned. Defaults to GraphML.</param> 
        /// <response code="200"> Returns the graph file in the response body</response>
        [HttpPost("process/{guid}")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public IActionResult Process([FromRoute] Guid guid, [FromQuery] ProcessMode mode = ProcessMode.GRID_BG, [FromQuery] GraphFormat format = GraphFormat.GraphML)
        {
            var userId = GetUserId();
            var result = _imageService.ProcessImageFile(guid, userId, mode);

            if (result)
            {
                var graphFile = _imageService.GetGraphFile(guid, userId, format);
                var stream = System.IO.File.OpenRead(graphFile.File.FullName);

                return File(stream, "application/octet-stream", graphFile.Name);
            }
            else
                return BadRequest(new ErrorMessage("Processing image gone wrong"));
        }

        /// <summary>
        ///     Downloads the desired graph file.
        /// </summary>
        /// <remarks>
        ///     Returns the desired graph file of the specified graph entity in a chosen file format.
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param> 
        /// <param name="format">Format, in which the processed graph file is returned. Defaults to GraphML.</param> 
        /// <response code="200"> Returns the graph file in the response body as "application/octet-stream"</response>
        /// <response code="400">Returns error message if the file is not found.</response>
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [HttpGet("get/{guid}")]
        public IActionResult Get(Guid guid, [FromQuery] GraphFormat format = GraphFormat.GraphML)
        {
            var userId = GetUserId();

            var graphFile = _imageService.GetGraphFile(guid, userId, format);

            if (graphFile == null)
                return BadRequest(new ErrorMessage("There is no such file"));

            var stream = System.IO.File.OpenRead(graphFile.File.FullName);

            return File(stream, "application/octet-stream", graphFile.Name);
        }

        /// <summary>
        /// Updates a graph file of the entity.   
        /// </summary>
        /// <remarks>
        /// If a image file was processed wrong, allows to update a graph file of the given graph entity by uploading a corrected graph file. 
        /// <para>TODO - validate if graph file is really graph file</para>
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param>
        /// <param name="file">Graph file in a graph format.</param>
        [HttpPut("update/{guid}")]
        public async Task<IActionResult> PutAsync([FromRoute] Guid guid, IFormFile file)
        {
            var userId = GetUserId();

            // TODO: validate if graph file is really graph file

            var result = await _imageService.UpdateGraphEntityAsync(guid, userId, file);

            if (result)
                return Ok();
            else
                return BadRequest(new ErrorMessage("Update gone wrong"));
        }

        /// <summary>
        /// Sets accessibility of a graph entity.
        /// </summary>
        /// <remarks>
        /// If a graph entity is set to public, it is available for other users to see, download and send to Drive. Once set to public, it stays on the server forever.
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param>
        /// <returns></returns>
        [HttpPut("set-public/{guid}")]
        public async Task<IActionResult> PutAsync([FromRoute] Guid guid)
        {
            var userId = GetUserId();

            var result = await _imageService.SetEntityAsPublicAsync(guid, userId);

            if (result)
                return Ok();
            else
                return BadRequest(new ErrorMessage("You don't have permission to update this graph."));
        }

        /// <summary>
        /// Deletes a graph entity.
        /// </summary>
        /// <remarks>
        /// Removes a certain graph entity from server's storage and user's history. If an entity was ever set public - it is removed only from user's own history.
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param>
        /// <returns></returns>
        [HttpDelete("delete/{guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid guid)
        {
            var userId = GetUserId();

            bool result = await _imageService.RemoveEntityAsync(guid, userId);

            if (result)
                return Ok(new { message = "success" });
            else
                return BadRequest(new ErrorMessage("Graph is already removed or you don't have permission to remove it."));
        }

        /// <summary>
        /// Gets user's history of graph entities.
        /// </summary>
        /// <remarks>
        /// Returns history of user's graph entities along with details.
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Returns a List of GraphEntity items</response>
        [ProducesResponseType(typeof(List<GraphEntity>), 200)]
        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var userId = GetUserId();

            var result = _imageService.GetHistory(userId);

            if (result != null)
                return Ok(result);
            else
                return BadRequest();
        }

        /// <summary>
        /// Gets recently created public graph entities.
        /// </summary>
        /// <remarks>
        /// Returns a list containing details of recently uploaded graph entities, that were ever made to public.
        /// </remarks>
        /// <param name="amount">Specifies amount of graph entities in a returned list.</param>
        /// <response code="200">Returns a List of GraphEntity items</response>
        [ProducesResponseType(typeof(List<GraphEntity>), 200)]
        [AllowAnonymous]
        [HttpGet("recent")]
        public IActionResult GetRecent([FromQuery] int amount = 100)
        {
            var result = _imageService.GetRecent(amount);

            if (result != null)
                return Ok(result);
            else
                return BadRequest();
        }

        // helper methods
        /// <summary>
        /// Return users id from ClaimsPrincipal.
        /// </summary>
        /// <returns></returns>
        private int GetUserId()
        {
            return Int32.Parse(User.Claims.ToList()[0].Value);
        }
    }
}
