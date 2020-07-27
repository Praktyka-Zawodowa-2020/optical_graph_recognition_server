using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    // TODO: Async methods
    // TODO: Proper error messages
    [Authorize]
    [ApiController]
    [Route("api/graphs")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessageResponse), 400)]
    public class GraphsController : ControllerBase
    {
        private readonly ImageValidator _imageValidator;
        private readonly IGraphService _graphService;

        public GraphsController(ImageValidator imageValidator, IGraphService graphService)
        {
            _imageValidator = imageValidator;
            _graphService = graphService;
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
        [HttpPost("create")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<IActionResult> CreateAsync([FromForm] CreateGraphRequest model)
        {
            var file = model.File;
            if (file == null)
                return BadRequest(new ErrorMessageResponse("No file provided"));
            if (file.Length == 0)
                return BadRequest(new ErrorMessageResponse("Empty file"));

            string validExtension = _imageValidator.GetValidExtension(file.OpenReadStream());
            if (validExtension.Equals(String.Empty))
                return BadRequest(new ErrorMessageResponse("Wrong file signature"));

            var userId = GetUserId();

            var guid = await _graphService.CreateGraphEntity(model, validExtension, userId);

            if (guid != null)
                return Ok(new { guid });
            else
                return BadRequest(new ErrorMessageResponse("Saving image gone wrong"));
        }

        /// <summary>
        /// Processes an image file of the graph entity. 
        /// </summary>
        /// <remarks>
        /// Takes an image file of the specified graph entity, processes it with a python script and returns graph file in a chosen file format. 
        /// Can be called repeatedly with different script parameters to get the best result from an image processing.
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param> 
        /// <param name="format">Format, in which the processed graph file is returned. Defaults to GraphML.</param>
        /// <param name="processRequest">Script parameters allowing to tweak processing to the actual needs.</param> 
        /// <response code="200"> Returns the graph file in the response body</response>
        [HttpPost("process/{guid}")]
        [Produces("application/octet-stream", "application/json")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public IActionResult Process([FromRoute] Guid guid, [FromBody] ProcessRequest processRequest, [FromQuery] GraphFormat format = GraphFormat.GraphML)
        {
            var userId = GetUserId();
            var result = _graphService.ProcessImageFile(guid, userId, processRequest);

            if (result)
            {
                var graphFile = _graphService.GetGraphFile(guid, userId, format);
                var stream = System.IO.File.OpenRead(graphFile.File.FullName);

                return File(stream, "application/octet-stream", graphFile.Name);
            }
            else
                return BadRequest(new ErrorMessageResponse("Processing image gone wrong"));
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
        [HttpGet("get/{guid}")]
        [Produces("application/octet-stream", "application/json")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public IActionResult Get(Guid guid, [FromQuery] GraphFormat format = GraphFormat.GraphML)
        {
            var userId = GetUserId();

            var graphFile = _graphService.GetGraphFile(guid, userId, format);

            if (graphFile == null)
                return BadRequest(new ErrorMessageResponse("There is no such file"));

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

            // TODO: Validate if graph file is really graph file

            var result = await _graphService.UpdateGraphEntityAsync(guid, userId, file);

            if (result)
                return Ok();
            else
                return BadRequest(new ErrorMessageResponse("Update gone wrong"));
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

            var result = await _graphService.SetEntityAsPublicAsync(guid, userId);

            if (result)
                return Ok();
            else
                return BadRequest(new ErrorMessageResponse("You don't have permission to update this graph."));
        }

        /// <summary>
        /// Deletes a graph entity.
        /// </summary>
        /// <remarks>
        /// Removes a certain graph entity from server's storage and user's history. If an entity was ever set to public - it is removed only from user's own history.
        /// </remarks>
        /// <param name="guid">GUID specyfying the graph entity.</param>
        /// <returns></returns>
        [HttpDelete("delete/{guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid guid)
        {
            var userId = GetUserId();

            bool result = await _graphService.RemoveEntityAsync(guid, userId);

            if (result)
                return Ok(new { message = "success" });
            else
                return BadRequest(new ErrorMessageResponse("Graph is already removed or you don't have permission to remove it."));
        }

        /// <summary>
        /// Gets user's history of graph entities.
        /// </summary>
        /// <remarks>
        /// Returns history of user's graph entities along with details.
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">Returns a List of GraphEntity items</response>
        [HttpGet("history")]
        [ProducesResponseType(typeof(List<GraphEntityDTO>), 200)]
        public IActionResult GetHistory()
        {
            var userId = GetUserId();

            var result = _graphService.GetHistory(userId);

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
        [AllowAnonymous]
        [HttpGet("recent")]
        [ProducesResponseType(typeof(List<GraphEntityDTO>), 200)]
        public IActionResult GetRecent([FromQuery] int amount = 100)
        {
            var result = _graphService.GetRecent(amount);

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
