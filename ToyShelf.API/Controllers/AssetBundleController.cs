using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.AssetBundle.Request;

namespace ToyShelf.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUpLoadController : ControllerBase
    {
        private readonly string _targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private readonly IProductColorService _productColorService;
        public FileUpLoadController(IProductColorService productColorService)
        {
            _productColorService = productColorService;
        }

        [RequestSizeLimit(500L * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500L * 1024 * 1024)]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadBundle([FromForm] FileUploadRequest request)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
                return BadRequest("Không có file nào được tải lên.");

            if (!Directory.Exists(_targetFolder))
                Directory.CreateDirectory(_targetFolder);

            string fileName = file.FileName.ToLower();
            string filePath = Path.Combine(_targetFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
           await _productColorService.UpdateFileProductColorAsync(fileName.ToUpper(), true);
             
            string fileUrl = $"/{fileName}";

            return Ok(new { success = true, message = $"Upload thành công: {fileName}", url = fileUrl });
        }

       
        [HttpDelete("delete/{sku}")]
        public async Task<IActionResult> DeleteBundle(string sku)
        {
            if (string.IsNullOrEmpty(sku)) return BadRequest("Thiếu mã SKU.");

            string fileName = sku.ToLower();
            string filePath = Path.Combine(_targetFolder, fileName);
            await _productColorService.UpdateFileProductColorAsync(fileName.ToUpper(), false);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Ok(new { success = true, message = $"Đã xóa file {fileName} ." });
            }
            
            return NotFound(new { success = false, message = "File không tồn tại." });
        }
    }
}
