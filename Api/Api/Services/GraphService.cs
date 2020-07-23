﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Helpers;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public class GraphService : IGraphService
    {
        private readonly DataContext _dataContext;
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        public GraphService(IOptions<AppSettings> appSettings,
                            ILogger<GraphService> logger,
                            DataContext dataContext)
        {
            _dataContext = dataContext;
            _logger = logger;
            _appSettings = appSettings.Value;
            _targetFilePath = _appSettings.StoragePaths.UploadsDirectory;
        }

        private readonly string _targetInFileName = "raw";
        private readonly string _targetFilePath;

        public GraphFile GetGraphFile(Guid guid, int userId, GraphFormat format)
        {
            var user = _dataContext.Users.SingleOrDefault(u => u.Id == userId);
            var entity = user.GraphEntities.SingleOrDefault(g => g.GUID.Equals(guid));
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
                if (format == GraphFormat.GraphML && item.Extension == (".graphml"))
                    file = item;
                else
                if (format == GraphFormat.Graph6 && item.Extension == (".g6"))
                    file = item;
            }
            if (file == null) return null;

            var graphFile = new GraphFile()
            {
                File = file,
                Name = entity.Name
            };

            return graphFile;
        }

        public bool ProcessImageFile(Guid guid, int userId, ProcessMode mode)
        {
            var user = _dataContext.Users.SingleOrDefault(u => u.Id == userId);
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
            var user = _dataContext.Users.SingleOrDefault(x => x.Id == userId);

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
                _dataContext.GraphEntities.Remove(entity);

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public IEnumerable<GraphEntity> GetHistory(int userId)
        {
            var user = _dataContext.Users.SingleOrDefault(u => u.Id == userId);
            return user.GraphEntities.ToList();
        }

        public IEnumerable<GraphEntity> GetRecent(int amount)
        {
            throw new NotImplementedException();
        }
    }
}
