using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.ProductCategory.Request;
using ToyCabin.Application.Models.ProductCategory.Response;
using ToyCabin.Application.Models.Role.Response;
using ToyCabin.Application.Models.User.Response;
using ToyCabin.Application.Services;

namespace ToyCabin.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductCategoryController : ControllerBase
	{
	    private readonly IProductCategoryService _productCategoryService;
		public ProductCategoryController(IProductCategoryService productCategoryService)
		{
			_productCategoryService = productCategoryService;
		}

		//==== CREATE =====
		/// <summary>
		/// Create Category.
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<BaseResponse<ProductCategoryResponse>>> CreateCategory(
		[FromBody] ProductCategoryRequest request)
		{
			var result = await _productCategoryService.CreateCategoryAsync(request);

			return BaseResponse<ProductCategoryResponse>.Ok(result, "Productcategory Created successfully");
		}

		// ===== GET Category PRODUCTS =====
		/// <summary>
		/// Gets Category.
		/// </summary>

		[HttpGet]
		public async Task<ActionResult<BaseResponse<IEnumerable<ProductCategoryResponse>>>> GetCategorys([FromQuery] bool? isActive)
		{
			var result = await _productCategoryService.GetCategoriesAsync(isActive);
			return BaseResponse<IEnumerable<ProductCategoryResponse>>
				.Ok(result, "Productcategory retrieved successfully");
		}

		// ===== UPDATE =====
		/// <summary>
		/// Update Category.
		/// </summary>
		[HttpPut("{id}")]
		public async Task<BaseResponse<ProductCategoryResponse?>> Update(
			Guid id,
			[FromBody] UpdateProductCategoryRequest request)
		{
			var result = await _productCategoryService
				.UpdateCategoryAsync(id, request);
			return BaseResponse<ProductCategoryResponse?>
				.Ok(result, "Product category updated successfully");
		}

		// ===== DISABLE =====
		/// <summary>
		/// Disable Category.
		/// </summary>
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _productCategoryService.DisableCategoryAsync(id);
			return ActionResponse.Ok("Productcategory disabled successfully");
		}

		// ===== RESTORE =====
		/// <summary>
		/// Restore Category.
		/// </summary>
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _productCategoryService.RestoreCategoryAsync(id);
			return ActionResponse.Ok("Productcategory disabled successfully");
		}

		// ===== DELETE =====
		/// <summary>
		/// Delete Category.
		/// </summary>
		[HttpDelete("{id}")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _productCategoryService.DeleteCategoryAsync(id);
			return ActionResponse.Ok("Productcategory delete successfully");
		}

		
	}
}
