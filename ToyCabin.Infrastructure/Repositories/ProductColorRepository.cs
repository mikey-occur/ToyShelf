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
	public class ProductColorRepository : GenericRepository<ProductColor>, IProductColorRepository
	{
		public ProductColorRepository(ToyCabinDbContext context) : base(context)
		{
		}

		public async Task<bool> ExistsBySkuAsync(string sku)
		{
			return await _context.ProductColors.AnyAsync(x => x.Sku == sku);
		}

		public Task<IEnumerable<ProductColor>> GetProductColorsAsync(bool? isActive)
		{
			var query = _context.ProductColors.AsQueryable();
			if (isActive.HasValue)
				query = query.Where(pc => pc.IsActive == isActive.Value);
			return Task.FromResult(query.AsEnumerable());
		}
	}
}
