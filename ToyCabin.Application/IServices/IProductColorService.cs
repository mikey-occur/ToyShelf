using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.ProductColor.Request;
using ToyCabin.Application.Models.ProductColor.Response;

namespace ToyCabin.Application.IServices
{
	public interface IProductColorService
	{
		Task<IEnumerable<ProductColorResponse>> GetProductColorsAsync(bool? isActive);
		Task<ProductColorResponse> GetByIdAsync(Guid id);
		Task<ProductColorResponse> CreateProductColorAsync(ProductColorRequest request);
		Task<ProductColorResponse?> UpdateProductColorAsync(Guid id, ProductColorUpdateRequest request);
		Task<bool> DeleteProductColorAsync(Guid id);

		Task<bool> RestoreProductColorAsync(Guid id);
		Task<bool> DisableProductColorAsync(Guid id);
	}
}
