using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;

namespace ToyShelf.Application.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly string _targetFolder;
        private readonly IProductColorService _productColorService;

        public FileUploadService(IProductColorService productColorService)
        {
            _productColorService = productColorService;
            _targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        public async Task<bool> UploadBundleAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)  
                throw new AppException("Không có file nào được tải lên.", 400); ;

            // 1. Trích xuất SKU từ tên file (ví dụ: A001.bundle -> SKU là A001)
            string sku = Path.GetFileNameWithoutExtension(file.FileName).ToUpper();

            // 2. KIỂM TRA SKU TRƯỚC KHI LƯU FILE
            // Nếu UpdateFileProductColorAsync trả về false (SKU không tồn tại), dừng lại ngay.
            var isSkuValid = await _productColorService.UpdateFileProductColorAsync(sku, true);
            if (!isSkuValid) 
                throw new AppException($"Upload thất bại. SKU '{sku}' không tồn tại trong hệ thống.", 400); 

            // 3. CHỈ KHI SKU TỒN TẠI MỚI TIẾN HÀNH GHI FILE VÀO WWWROOT
            if (!Directory.Exists(_targetFolder))
                Directory.CreateDirectory(_targetFolder);

            string fileName = file.FileName.ToLower();
            string filePath = Path.Combine(_targetFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return true;
        }

        public async Task<bool> DeleteBundleAsync(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                throw new AppException("Thiếu mã SKU.", 400); 

            string fileName = sku.ToLower();
            string filePath = Path.Combine(_targetFolder, fileName);

            // Cập nhật Database và xóa file vật lý
            var isUpdated = await _productColorService.UpdateFileProductColorAsync(sku.ToUpper(), false);
            if(!isUpdated)
                throw new AppException($"Xóa thất bại. SKU '{sku}' không tồn tại trong hệ thống.", 400);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return isUpdated;
        }
    }
}
