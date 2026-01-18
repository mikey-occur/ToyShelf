using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;
using ToyCabin.Infrastructure.Context;

namespace ToyCabin.Infrastructure.Repositories
{
	public class ProductCategoryRepository : GenericRepository<ProductCategory>, IProductCategoryRepository
	{
		public ProductCategoryRepository(ToyCabinDbContext context) : base(context)
		{
		}

		public async Task<ProductCategory?> GetByNameAsync(string name)
		{
			return await _context.ProductCategories
				.FirstOrDefaultAsync(pc => pc.Name == name);
		}

		public async Task<IEnumerable<ProductCategory>> GetProductCategoriesAsync(bool? isActive)
		{
			var query = _context.ProductCategories.AsQueryable();
			if (isActive.HasValue)
				query = query.Where(pc => pc.IsActive == isActive.Value);

			return await query.ToListAsync();
		}

	}
}
