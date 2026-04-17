using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ToyShelf.API.Hubs;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Product.Request;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.ProductCategory.Request;
using ToyShelf.Application.Models.ProductCategory.Response;
using ToyShelf.Application.Models.ProductColor.Request;
using ToyShelf.Application.Models.ProductColor.Response;
using ToyShelf.Application.Services;

namespace ToyShelf.API.Controllers

{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductColorController : ControllerBase
	{
		private readonly IProductColorService _productColorService;
		private readonly IHubContext<ProductHub> _hubContext;
		public ProductColorController(IProductColorService productColorService, IHubContext<ProductHub> hubContext)
		{
			_productColorService = productColorService;
			_hubContext = hubContext;
		}

		//==== CREATE =====
		/// <summary>
		/// Create ProductColor.
		/// </summary>
		[HttpPost]
		public async Task<BaseResponse<ProductColorResponse>> CreateCategory(
		[FromBody] ProductColorRequest request)
		{
			var result = await _productColorService.CreateProductColorAsync(request);

			return BaseResponse<ProductColorResponse>.Ok(result, "Productcategory Created successfully");
		}

		// ===== GET ProductColor PRODUCTS =====
		/// <summary>
		/// Get ProductColor product.
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ProductColorResponse>>> GetProductColors([FromQuery] bool? isActive)
		{
			var result = await _productColorService.GetProductColorsAsync(isActive);
			return BaseResponse<IEnumerable<ProductColorResponse>>.Ok(result, "ProductColor products retrieved successfully");
		}

		/// <summary>
		/// Get by id.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<ProductColorResponse?>> GetById(Guid id)
		{
			var result = await _productColorService.GetByIdAsync(id);
			return BaseResponse<ProductColorResponse?>.Ok(result, "Product retrieved successfully");
		}

		/// <summary>
		/// Tìm kiếm ProductColor variant theo một phần của SKU. 
		/// </summary>
		[HttpGet("variant/search")]
		public async Task<BaseResponse<List<ProductBySkuResponse>>> SearchByVariantSku([FromQuery] string keyword)
		{
			// Gọi hàm Search thay vì hàm Get cũ
			var result = await _productColorService.SearchByVariantSkuAsync(keyword);

			// Trả về List kèm message thành công
			return BaseResponse<List<ProductBySkuResponse>>.Ok(result, "Tìm kiếm biến thể thành công");
		}

		/// <summary>
		/// Update ProductColor.
		/// </summary>
		// ===== UPDATE PRODUCT color =====
		[HttpPut("{id}")]
		public async Task<BaseResponse<ProductColorResponse?>> Update(Guid id, [FromBody] ProductColorUpdateRequest request)
		{
			var result = await _productColorService.UpdateProductColorAsync(id, request);

			return BaseResponse<ProductColorResponse?>.Ok(result, "Product updated successfully");
		}

		/// <summary>
		/// Delete Productcolor.
		/// </summary>
		// ===== DELETE PRODUCT COLOR =====
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _productColorService.DeleteProductColorAsync(id);
			return ActionResponse.Ok("Productcolor delete successfully");
		}
		/// <summary>
		/// Disable ProductColor.
		/// </summary>
		// ===== DISABLE PRODUCTCOLOR =====
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _productColorService.DisableProductColorAsync(id);
			return ActionResponse.Ok("Productcolor disabled successfully");
		}
		/// <summary>
		/// Restore ProductColor.
		/// </summary>
		// ===== RESTORE PRODUCTColor =====
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _productColorService.RestoreProductColorAsync(id);
			return ActionResponse.Ok("Productcolor restore successfully");
		}

		[HttpPost("select")]
        public async Task<IActionResult> SelectProduct([FromBody]string skuCode)
        {
            if (string.IsNullOrEmpty(skuCode))
            {
                return BadRequest("SkuCode không được để trống.");
            }
            await _hubContext.Clients.All.SendAsync("OnProductSelected", skuCode);

            return Ok(new
            {
                success = true,
                message = $"Đã gửi lệnh hiển thị sản phẩm: {skuCode}",
                timestamp = System.DateTime.Now
            });
        }

        [HttpPost("{rotationDegree}")]
        public async Task<IActionResult> RotateProduct(int rotationDegree)
        {
            await _hubContext.Clients.All.SendAsync("OnProductRotated", rotationDegree);

            return Ok(new
            {
                success = true,
                message = $"Đã gửi lệnh xoay model một góc: {rotationDegree} độ"
            });


        }

    }
}
