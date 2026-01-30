using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Product.Request;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.Store.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
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
		Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetProductsPaginatedAsync(int pageNumber = 1,int pageSize = 10,bool? isActive = null,Guid? categoryId = null);


    }
}
