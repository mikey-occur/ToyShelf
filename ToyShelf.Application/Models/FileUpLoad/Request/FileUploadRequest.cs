using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.AssetBundle.Request
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; } = null!;
    
    }
}
