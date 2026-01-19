using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.ProductCategory.Request;
using ToyCabin.Application.Models.ProductCategory.Response;
using ToyCabin.Application.Models.ProductColor.Request;
using ToyCabin.Application.Models.ProductColor.Response;
using ToyCabin.Application.Services;

namespace ToyCabin.API.Controllers

{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductColorController : ControllerBase
	{
		private readonly IProductColorService _productColorService;

		public ProductColorController(IProductColorService productColorService)
		{
			_productColorService = productColorService;
		}

		//==== CREATE =====
		/// <summary>
		/// Create ProductColor.
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<BaseResponse<ProductColorResponse>>> CreateCategory(
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
		/// Delete Product.
		/// </summary>
		// ===== DELETE PRODUCT =====
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _productColorService.DeleteProductColorAsync(id);
			return ActionResponse.Ok("Productcolor delete successfully");
		}
		/// <summary>
		/// Disable Product.
		/// </summary>
		// ===== DISABLE PRODUCT =====
		[HttpPost("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _productColorService.DisableProductColorAsync(id);
			return ActionResponse.Ok("Productcolor disabled successfully");
		}
		/// <summary>
		/// Restore Product.
		/// </summary>
		// ===== RESTORE PRODUCT =====
		[HttpPost("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _productColorService.RestoreProductColorAsync(id);
			return ActionResponse.Ok("Productcolor restore successfully");
		}
	}
}
