using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IProductRepository : IGenericRepository<Product>
	{
		Task<int> GetNextSequenceAsync(string categoryCode);
		Task<IEnumerable<Product>> GetProductsAsync(bool? isActive);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsPaginatedAsync(int pageNumber = 1,int pageSize = 10,bool? isActive = null, Guid? categoryId = null);
		Task<IEnumerable<Product>> SearchAsync(string keyword, bool? isActive);
	}
}
