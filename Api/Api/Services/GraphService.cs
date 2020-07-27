using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Helpers;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public class GraphService : IGraphService
    {
        private readonly DataContext _dataContext;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public GraphService(IOptions<AppSettings> appSettings,
                            ILogger<GraphService> logger,
                            DataContext dataContext,
                            IMapper mapper)
        {
            _dataContext = dataContext;
            _appSettings = appSettings.Value;
            _logger = logger;
            _mapper = mapper;
            _targetFilePath = _appSettings.StoragePaths.UploadsDirectory;
        }

        private readonly string _targetInFileName = "raw";
        private readonly string _targetFilePath;

        public GraphFileDTO GetGraphFile(Guid guid, int userId, GraphFormat format)
        {
            var entity = _dataContext.GraphEntities
                .Include(g => g.Owner)
                .SingleOrDefault(g => 
                g.GUID.Equals(guid) && (g.Owner.Id == userId || g.IsPublic));
            if (entity == null) return null;

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_targetFilePath, guid.ToString()));
            if (!dir.Exists)
                return null;

            FileInfo[] files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);

            FileInfo file = null;
            foreach (var item in files)
            {
                if (format == GraphFormat.RawImage && Path.GetFileNameWithoutExtension(item.Name).Equals(_targetInFileName))
                    file = item;
                else
                if (format == GraphFormat.GraphML && item.Extension.Equals(Strings.GRAPHML))
                    file = item;
                else
                if (format == GraphFormat.Graph6 && item.Extension.Equals(Strings.G6))
                    file = item;
            }
            if (file == null) return null;

            var graphFile = new GraphFileDTO()
            {
                File = file,
                Name = string.Concat(entity.Name, file.Extension)
            };

            return graphFile;
        }

        public bool ProcessImageFile(Guid guid, int userId, ProcessRequest model)
        {
            var user = _dataContext.Users.Include(u => u.GraphEntities).SingleOrDefault(u => u.Id == userId);
            var entity = user.GraphEntities.SingleOrDefault(g => g.GUID.Equals(guid));
            if (entity == null) return false;

            var image = GetGraphFile(guid, userId, GraphFormat.RawImage).File;
            var script = _appSettings.StoragePaths.ScriptFullPath;
            var param = "-p " + image.FullName;// + " -b " + (int) mode;
            var result = new PythonRunner().Run(script, param);
            _logger.LogInformation("Processing image result:", result);
            return true;
        }

        public async Task<Guid> CreateGraphEntity(CreateGraphRequest model, string validExtension, int userId)
        {
            var file = model.File;
            var user = _dataContext.Users.Include(u => u.GraphEntities).SingleOrDefault(x => x.Id == userId);

            Guid guid = Guid.NewGuid();
            var path = Path.Combine(_targetFilePath, guid.ToString());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetInFileName, validExtension)), FileMode.Create))
                await file.CopyToAsync(targetStream);

            user.GraphEntities.Add(new GraphEntity()
            {
                CreatedAt = DateTime.Today,
                Owner = user,
                GUID = guid,
                Name = model.Name
            });

            var result = await _dataContext.SaveChangesAsync();

            return guid;
        }

        public async Task<bool> SetEntityAsPublicAsync(Guid guid, int userId)
        {
            var entity = _dataContext.GraphEntities.SingleOrDefault(g => g.GUID.Equals(guid) && g.Owner.Id == userId);
            if (entity == null) return false;

            entity.IsPublic = true;

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveEntityAsync(Guid guid, int userId)
        {
            var user = _dataContext.Users.SingleOrDefault(u => u.Id == userId);
            var entity = _dataContext.GraphEntities.SingleOrDefault(g => g.GUID.Equals(guid) && g.Owner.Id == userId);
            if (entity == null) return false;

            user.GraphEntities.Remove(entity);
            if (!entity.IsPublic)
            {
                _dataContext.GraphEntities.Remove(entity);

                var path = Path.Combine(_appSettings.StoragePaths.UploadsDirectory, guid.ToString());
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public IEnumerable<GraphEntityDTO> GetHistory(int userId)
        {
            var user = _dataContext.Users.Include(u => u.GraphEntities).SingleOrDefault(u => u.Id == userId);
            var entities = user.GraphEntities.ToList();
            var result = _mapper.Map<List<GraphEntity>, List<GraphEntityDTO>>(entities);
            return result;
        }

        public IEnumerable<GraphEntityDTO> GetRecent(int amount)
        {
            var entities = _dataContext.GraphEntities.Where(e => e.IsPublic).Include(e => e.Owner).OrderBy(e => e.CreatedAt).Take(amount).ToList();
            var result = _mapper.Map<List<GraphEntity>, List<GraphEntityDTO>>(entities);
            return result;
        }

        public async Task<bool> UpdateGraphEntityAsync(Guid guid, int userId, IFormFile file)
        {
            var entity = _dataContext.GraphEntities.Include(g=>g.Owner).SingleOrDefault(g => g.GUID.Equals(guid) && g.Owner.Id == userId);
            if (entity == null) return false;

            var path = Path.Combine(_targetFilePath, guid.ToString());
            if (!Directory.Exists(path))
                return false;

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetInFileName, Path.GetExtension(file.FileName))), FileMode.Truncate))
                await file.CopyToAsync(targetStream);

            return true;
        }
    }
}
