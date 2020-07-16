using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class GetImageRequest
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
    }
}
