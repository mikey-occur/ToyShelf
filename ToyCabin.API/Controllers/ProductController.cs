using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.Store.Response;
using ToyCabin.Application.Services;

namespace ToyCabin.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductController : ControllerBase
	{
        private readonly IProductService _productService;
		public ProductController(IProductService productService)
		{
			   _productService = productService;
		}


		// ===== GET ALL PRODUCTS =====
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ProductResponse>>> GetAll()
		{
			var result = await _productService.GetAllProductsAsync();
			return BaseResponse<IEnumerable<ProductResponse>>.Ok(result, "Product retrieved successfully");
		}

		// ===== GET ACTIVE PRODUCTS =====
		[HttpGet("active")]
		public async Task<BaseResponse<IEnumerable<ProductResponse>>> GetActive()
		{
			var result = await _productService.GetActiveProductsAsync();
			return BaseResponse<IEnumerable<ProductResponse>>.Ok(result, "Active products retrieved successfully");
		}

		// ===== GET INACTIVE PRODUCTS =====
		[HttpGet("inactive")]
		public async Task<BaseResponse<IEnumerable<ProductResponse>>> GetInactive()
		{
			var result = await _productService.GetInactiveProductsAsync();
			return BaseResponse<IEnumerable<ProductResponse>>.Ok(result, "Inactive products retrieved successfully");
		}

		// ===== CREATE PRODUCT =====
		[HttpPost]
		public async Task<BaseResponse<ProductResponse>> Create([FromBody] ProductRequest request)
		{
			var result = await _productService.CreateProductAsync(request);
			return BaseResponse<ProductResponse>.Ok(result, "Product created successfully");
		}

		// ===== UPDATE PRODUCT =====
		[HttpPut("{id}")]
		public async Task<BaseResponse<ProductResponse?>> Update(Guid id, [FromBody] ProductUpdateRequest request)
		{
			var result = await _productService.UpdateProductAsync(id, request);
		
			return BaseResponse<ProductResponse?>.Ok(result, "Product updated successfully");
		}

		// ===== DELETE PRODUCT =====
		[HttpDelete("{id}")]
		public async Task<BaseResponse<bool>> Delete(Guid id)
		{
			var result = await _productService.DeleteProductAsync(id);
			return BaseResponse<bool>.Ok(result, "Product deleted successfully");
		}

		// ===== DISABLE PRODUCT =====
		[HttpPost("{id}/disable")]
		public async Task<BaseResponse<bool>> Disable(Guid id)
		{
			var result = await _productService.DisableProductAsync(id);
			return BaseResponse<bool>.Ok(result, "Product disabled successfully");
		}

		// ===== RESTORE PRODUCT =====
		[HttpPost("{id}/restore")]
		public async Task<BaseResponse<bool>> Restore(Guid id)
		{
			var result = await _productService.RestoreProductAsync(id);
			return BaseResponse<bool>.Ok(result, "Product restored successfully");
		}
	}
}
