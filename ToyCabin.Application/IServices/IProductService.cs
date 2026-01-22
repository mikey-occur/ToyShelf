using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.Store.Response;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Application.IServices
{
	public interface IProductService
	{
		Task<IEnumerable<ProductResponse>> GetAllProductsAsync();
		Task<IEnumerable<ProductResponse>> GetActiveProductsAsync();
		Task<IEnumerable<ProductResponse>> GetInactiveProductsAsync();
		Task<IEnumerable<ProductResponse>> GetProductsAsync(bool? isActive);
		Task<ProductResponse> GetByIdAsync(Guid id);
		Task<ProductResponse> CreateProductAsync(ProductRequest request);
		Task<ProductResponse?> UpdateProductAsync(Guid id, ProductUpdateRequest request);
		Task<bool> DeleteProductAsync(Guid id);

		Task<bool> RestoreProductAsync(Guid id);
		Task<bool> DisableProductAsync(Guid id);
		Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetProductsPaginatedAsync(int pageNumber = 1,int pageSize = 10,bool? isActive = null,Guid? categoryId = null);


    }
}
