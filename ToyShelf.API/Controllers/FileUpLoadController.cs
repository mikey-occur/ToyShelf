using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.AssetBundle.Request;

namespace ToyShelf.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUpLoadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUpLoadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// Upload asset bundle product color.
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ActionResponse>> UploadBundle([FromForm] FileUploadRequest request)
        {
            var isSuccess = await _fileUploadService.UploadBundleAsync(request.File);          
            return ActionResponse.Ok("Upload file và cập nhật trạng thái thành công.");
        }

        /// <summary>
        /// Delete asset bundle product color.
        /// </summary>
        [HttpDelete("delete/{sku}")]
        public async Task<ActionResult<ActionResponse>> DeleteBundle(string sku)
        {
            var isSuccess = await _fileUploadService.DeleteBundleAsync(sku);
            return ActionResponse.Ok($"Đã xóa file và cập nhật trạng thái cho SKU: {sku}");
        }
    }
}
