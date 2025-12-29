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
		[HttpPost]
		public async Task<ActionResult<BaseResponse<ProductCategoryResponse>>> CreateCategory(
		[FromBody] ProductCategoryRequest request)
		{
			var result = await _productCategoryService.CreateCategoryAsync(request);

			return BaseResponse<ProductCategoryResponse>.Ok(result, "Productcategory Created successfully");
		}

		// ===== GET ALL =====
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ProductCategoryResponse>>> GetAll()
		{
			var result = await _productCategoryService
				.GetAllCategoriesAsync();

			return BaseResponse<IEnumerable<ProductCategoryResponse>>
				.Ok(result , "Productcategory retrieved successfully");
		}

		// ===== GET ACTIVE =====
		[HttpGet("active")]
		public async Task<BaseResponse<IEnumerable<ProductCategoryResponse>>> GetActive()
		{
			var result = await _productCategoryService
				.GetActiveCategoriesAsync();

			return BaseResponse<IEnumerable<ProductCategoryResponse>>
				.Ok(result, "Productcategory active successfully");
		}

		// ===== GET INACTIVE =====
		[HttpGet("inactive")]
		public async Task<BaseResponse<IEnumerable<ProductCategoryResponse>>> GetInactive()
		{
			var result = await _productCategoryService
				.GetinactiveCategoriesAsync();

			return BaseResponse<IEnumerable<ProductCategoryResponse>>
				.Ok(result, "Productcategory inactive successfully");
		}

		// ===== UPDATE =====
		[HttpPut("{id}")]
		public async Task<BaseResponse<ProductCategoryResponse?>> Update(
			Guid id,
			[FromBody] ProductCategoryRequest request)
		{
			var result = await _productCategoryService
				.UpdateCategoryAsync(id, request);
			return BaseResponse<ProductCategoryResponse?>
				.Ok(result, "Product category updated successfully");
		}

		// ===== DISABLE =====
		[HttpPatch("{id}/disable")]
		public async Task<BaseResponse<bool>> Disable(Guid id)
		{
			var success = await _productCategoryService
				.DisableCategoryAsync(id);
			return BaseResponse<bool>
				.Ok(true, "Productcategory disabled successfully");
		}

		// ===== RESTORE =====
		[HttpPatch("{id}/restore")]
		public async Task<BaseResponse<bool>> Restore(Guid id)
		{
			var success = await _productCategoryService
				.RestoreCategoryAsync(id);
			return BaseResponse<bool>
				.Ok(true, "Productcategory restored successfully");
		}

		// ===== DELETE =====
		[HttpDelete("{id}")]
		public async Task<BaseResponse<bool>> Delete(Guid id)
		{
			var success = await _productCategoryService
				.DeleteCategoryAsync(id);
			return BaseResponse<bool>
				.Ok(true, "Productcategory deleted successfully");
		}

	}
}
