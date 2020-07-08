using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
    public interface IImageValidator
    {
        public bool IsValid(Stream stream, string extension);
    }
}
