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

        public GraphService(
            IOptions<AppSettings> appSettings,
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

        private readonly string _targetInFileName = Strings.RAW;
        private readonly string _targetFilePath;

        public GraphFileDTO GetGraphFile(Guid guid, GraphFormat format)
        {
            var entity = _dataContext.GraphEntities
                .Include(g => g.Owner)
                .SingleOrDefault(g =>
                g.GUID.Equals(guid));
            if (entity == null)
                return null;

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_targetFilePath, guid.ToString()));
            if (!dir.Exists)
                return null;

            FileInfo[] files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);

            FileInfo file = null;
            foreach (var item in files)
            {
                if (format == GraphFormat.RawImage 
                    && Path.GetFileNameWithoutExtension(item.Name).Equals(_targetInFileName)
                    && !item.Extension.Equals(Strings.EXT_GRAPHML) //temp
                    && !item.Extension.Equals(Strings.EXT_G6)      //fix
                    )
                    file = item;
                else
                if (format == GraphFormat.GraphML && item.Extension.Equals(Strings.EXT_GRAPHML))
                    file = item;
                else
                if (format == GraphFormat.Graph6 && item.Extension.Equals(Strings.EXT_G6))
                    file = item;
            }
            if (file == null)
                return null;

            var graphFile = new GraphFileDTO()
            {
                File = file,
                Name = string.Concat(entity.Name, file.Extension)
            };

            return graphFile;
        }

        public ProcessImageResult ProcessImageFile(Guid guid, ProcessRequest model)
        {
            var image = GetGraphFile(guid, GraphFormat.RawImage).File;

            var script = _appSettings.StoragePaths.ScriptFullPath;
            var param = "-p " + image.FullName + " -b " + model.Mode.ToString();
            _logger.LogInformation("SCRIPT PARAMETERS: " + param);

            var processResult = new PythonRunner().Run(script, param);
            _logger.LogInformation("Processing image result:\n " + processResult);

            var result = new ProcessImageResult();
            if(processResult[0] == '0')
                result.Succeed = true;
            else
            {
                result.Succeed = false;
                result.ErrorMessage = processResult;
            }

            return result;
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

        public async Task<bool> SetEntityAsPublicAsync(Guid guid)
        {
            var entity = _dataContext.GraphEntities.SingleOrDefault(g => g.GUID.Equals(guid));
            if (entity == null) return false;

            entity.IsPublic = true;

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveEntityAsync(Guid guid)
        {
            var entity = _dataContext.GraphEntities.SingleOrDefault(g => g.GUID.Equals(guid));

            var user = _dataContext.GraphEntities.Include(g => g.Owner).SingleOrDefault(g => g.GUID.Equals(guid)).Owner;
            if (user != null) user.GraphEntities.Remove(entity);

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

        public async Task<bool> UpdateGraphEntityAsync(Guid guid, IFormFile file)
        {
            var path = Path.Combine(_targetFilePath, guid.ToString());
            if (!Directory.Exists(path))
                return false;

            using (var targetStream = new FileStream(Path.Combine(path, string.Concat(_targetInFileName, Path.GetExtension(file.FileName))), FileMode.Create))
                await file.CopyToAsync(targetStream);

            return true;
        }

        public bool CheckOwnership(Guid guid, int userId)
        {
            var entity = _dataContext.GraphEntities.Include(g => g.Owner).SingleOrDefault(g => g.GUID.Equals(guid) && g.Owner.Id == userId);
            return !(entity == null);
        }

        public bool CheckOwnershipAndPublicity(Guid guid, int userId)
        {
            var entity = _dataContext.GraphEntities.Include(g => g.Owner).SingleOrDefault(g => g.GUID.Equals(guid) && (g.Owner.Id == userId || g.IsPublic));
            return !(entity == null);
        }
    }
}
