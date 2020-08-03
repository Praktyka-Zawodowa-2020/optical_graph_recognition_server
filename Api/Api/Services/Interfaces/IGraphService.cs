using Api.DTOs;
using Api.Helpers;
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
        Task<bool> SetEntityAsPublicAsync(Guid guid);
        Task<bool>UpdateGraphEntityAsync(Guid guid, IFormFile file);
        Task<bool> RemoveEntityAsync(Guid guid);
        Task<bool> RemoveAllUsersEntitiesAsync(int userId);
        ProcessImageResult ProcessImageFile(Guid guid, ProcessRequest model);
        GraphFileDTO GetGraphFile(Guid guid, GraphFormat format);
        IEnumerable<GraphEntityDTO> GetHistory(int userId);
        IEnumerable<GraphEntityDTO> GetRecent(int amount);
        bool CheckOwnership(Guid guid, int userId);
        bool CheckOwnershipAndPublicity(Guid guid, int userId);
    }
}
