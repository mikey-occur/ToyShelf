using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Domain.IRepositories
{
	public interface IProductColorRepository : IGenericRepository<ProductColor>
	{
		Task<IEnumerable<ProductColor>> GetProductColorsAsync(bool? isActive);
		Task<bool> ExistsBySkuAsync(string sku);
		Task<ProductColor?> GetColorBySkuAsync(string sku);
	}
}
