using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.ProductCategory.Request;
using ToyShelf.Application.Models.ProductCategory.Response;
using ToyShelf.Application.Models.Store.Response;

namespace ToyShelf.Application.IServices
{
	public interface IProductCategoryService
	{
		Task<ProductCategoryResponse> CreateCategoryAsync(ProductCategoryRequest request);
		Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid id, UpdateProductCategoryRequest request);
		Task<IEnumerable<ProductCategoryResponse>> GetCategoriesAsync(bool? isActive);
		Task DeleteCategoryAsync(Guid id);
		Task RestoreCategoryAsync(Guid id);
		Task DisableCategoryAsync(Guid id);

	}
}
