using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.ProductCategory.Request;
using ToyCabin.Application.Models.ProductCategory.Response;

namespace ToyCabin.Application.IServices
{
	public interface IProductCategoryService
	{
		Task<ProductCategoryResponse> CreateCategoryAsync(ProductCategoryRequest request);
		Task<IEnumerable<ProductCategoryResponse>> GetAllCategoriesAsync();
		Task<IEnumerable<ProductCategoryResponse>> GetActiveCategoriesAsync();
		Task<IEnumerable<ProductCategoryResponse>> GetinactiveCategoriesAsync();
		Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid id, ProductCategoryRequest request);
		Task<bool> DeleteCategoryAsync(Guid id);
		Task<bool> RestoreCategoryAsync(Guid id);
		Task<bool> DisableCategoryAsync(Guid id);

	}
}
