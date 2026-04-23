using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.IServices
{
    public interface IFileUploadService
    {
        Task<bool> UploadBundleAsync(IFormFile file);
        Task<bool> DeleteBundleAsync(string sku);
    }
}
