using Api.DTOs;
using Api.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IGraphService
    {
        Task<Guid> CreateGraphEntity(CreateGraphRequest model, string validExtension, int userId);
        /// <summary>
        /// Returns GraphFile of the specific Graph Entity. Contains FileInfo about actual file in the storage along with its entity meta name.
        /// </summary>
        /// <param name="guid">GUID identifying the graph entity</param>
        /// <param name="userId">If entity is not public, owner's id of the graph entity.</param>
        /// <returns>GraphFile</returns>
        Task<bool> SetEntityAsPublicAsync(Guid guid, int userId);
        Task<bool>UpdateGraphEntityAsync(Guid guid, int userId, IFormFile file);
        Task<bool> RemoveEntityAsync(Guid guid, int userId);
        bool ProcessImageFile(Guid guid, int userId, ProcessRequest model);
        GraphFileDTO GetGraphFile(Guid guid, int userId, GraphFormat format);
        IEnumerable<GraphEntityDTO> GetHistory(int userId);
        IEnumerable<GraphEntityDTO> GetRecent(int amount);
    }
}
