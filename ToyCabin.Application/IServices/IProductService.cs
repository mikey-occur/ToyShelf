using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;

namespace ToyCabin.Application.IServices
{
	public interface IProductService
	{
		Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
		Task<IEnumerable<ProductResponse>> GetActiveProductsAsync();
		Task<IEnumerable<ProductResponse>> GetInactiveProductsAsync();
        
		Task<ProductResponse> CreateProductAsync(ProductRequest request);
		Task<ProductResponse?> UpdateProductAsync(Guid id, ProductUpdateRequest request);
		Task<bool> DeleteProductAsync(Guid id);

		Task<bool> RestoreProductAsync(Guid id);
		Task<bool> DisableProductAsync(Guid id);

	}
}
