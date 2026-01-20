using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.Store.Response;

namespace ToyCabin.Application.IServices
{
	public interface IProductService
	{
		Task<IEnumerable<ProductResponse>> GetProductsAsync(bool? isActive);
		Task<ProductResponse> GetByIdAsync(Guid id);
		Task<ProductResponse> CreateProductAsync(ProductRequest request);
		Task<ProductResponse?> UpdateProductAsync(Guid id, ProductUpdateRequest request);
		Task<bool> DeleteProductAsync(Guid id);
		Task<IEnumerable<ProductResponse>> SearchAsync(string keyword, bool? isActive);
		Task<bool> RestoreProductAsync(Guid id);
		Task<bool> DisableProductAsync(Guid id);

	}
}
