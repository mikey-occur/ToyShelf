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

		/// <summary>
		/// Create Product.
		/// </summary>

		// ===== CREATE PRODUCT =====
		[HttpPost]
		public async Task<BaseResponse<ProductResponse>> Create([FromBody] ProductRequest request)
		{
			var result = await _productService.CreateProductAsync(request);
			return BaseResponse<ProductResponse>.Ok(result, "Product created successfully");
		}

		// ===== Get PRODUCTS =====
		/// <summary>
		/// Get product.
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ProductResponse>>> GetProducts([FromQuery] bool? isActive)
		{
			var result = await _productService.GetProductsAsync(isActive);
			return BaseResponse<IEnumerable<ProductResponse>>.Ok(result, "Products retrieved successfully");
		}
		/// <summary>
		/// Get by id.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<ProductResponse?>> GetById(Guid id)
		{
			var result = await _productService.GetByIdAsync(id);
			return BaseResponse<ProductResponse?>.Ok(result, "Product retrieved successfully");
		}


		/// <summary>
		/// Search Product byname key word.
		/// </summary>
		// ===== Search PRODUCT =====
		[HttpGet("search")]
		public async Task<BaseResponse<IEnumerable<ProductResponse>>> Search([FromQuery] string keyword, [FromQuery] bool? isActive)
		{
			var result = await _productService.SearchAsync(keyword, isActive);

			return BaseResponse<IEnumerable<ProductResponse>>.Ok(result);
		}

		/// <summary>
		/// Update Product.
		/// </summary>
		// ===== UPDATE PRODUCT =====
		[HttpPut("{id}")]
		public async Task<BaseResponse<ProductResponse?>> Update(Guid id, [FromBody] ProductUpdateRequest request)
		{
			var result = await _productService.UpdateProductAsync(id, request);
		
			return BaseResponse<ProductResponse?>.Ok(result, "Product updated successfully");
		}
		/// <summary>
		/// Delete Product.
		/// </summary>
		// ===== DELETE PRODUCT =====
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _productService.DeleteProductAsync(id);
			return ActionResponse.Ok("Product delete successfully");
		}
		/// <summary>
		/// Disable Product.
		/// </summary>
		// ===== DISABLE PRODUCT =====
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _productService.DisableProductAsync(id);
			return ActionResponse.Ok("Product disabled successfully");
		}
		/// <summary>
		/// Restore Product.
		/// </summary>
		// ===== RESTORE PRODUCT =====
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _productService.RestoreProductAsync(id);
			return ActionResponse.Ok("Product restore successfully");
		}


	}
}
