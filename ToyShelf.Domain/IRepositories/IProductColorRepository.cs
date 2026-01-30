using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IProductColorRepository : IGenericRepository<ProductColor>
	{
		Task<IEnumerable<ProductColor>> GetProductColorsAsync(bool? isActive);
		Task<bool> ExistsBySkuAsync(string sku);
		Task<ProductColor?> GetColorBySkuAsync(string sku);
	}
}
