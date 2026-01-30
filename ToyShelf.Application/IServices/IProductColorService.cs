using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Product.Request;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.ProductColor.Request;
using ToyShelf.Application.Models.ProductColor.Response;

namespace ToyShelf.Application.IServices
{
	public interface IProductColorService
	{
		Task<IEnumerable<ProductColorResponse>> GetProductColorsAsync(bool? isActive);
		Task<ProductColorResponse> GetByIdAsync(Guid id);
		Task<ProductColorResponse> CreateProductColorAsync(ProductColorRequest request);
		Task<ProductColorResponse?> UpdateProductColorAsync(Guid id, ProductColorUpdateRequest request);
		Task<bool> DeleteProductColorAsync(Guid id);
		Task<ProductBySkuResponse?> GetByVariantSkuAsync(string sku);
		Task<bool> RestoreProductColorAsync(Guid id);
		Task<bool> DisableProductColorAsync(Guid id);
	}
}
