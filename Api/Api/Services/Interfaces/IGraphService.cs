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
        /// <param name="format">Specifies which file (in which format) to return.</param>
        /// <returns>GraphFile</returns>
        GraphFile GetGraphFile(Guid guid, int userId, GraphFormat format);
        bool ProcessImageFile(Guid guid, int userId, ProcessMode mode);
        Task<bool> SetEntityAsPublicAsync(Guid guid, int userId);
        Task<bool> RemoveEntityAsync(Guid guid, int userId);
        IEnumerable<GraphEntity> GetHistory(int userId);
        IEnumerable<GraphEntity> GetRecent(int amount);
    }
}
