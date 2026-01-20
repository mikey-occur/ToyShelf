using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Domain.IRepositories
{
	public interface IProductRepository : IGenericRepository<Product>
	{
		Task<int> GetNextSequenceAsync(string categoryCode);
		Task<IEnumerable<Product>> GetProductsAsync(bool? isActive);
		Task<IEnumerable<Product>> SearchAsync(string keyword, bool? isActive);
	}
}
