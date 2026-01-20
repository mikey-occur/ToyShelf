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
	public class ProductRepository : GenericRepository<Product>, IProductRepository
	{
		public ProductRepository(ToyCabinDbContext context) : base(context)
		{
		}

		public async Task<int> GetNextSequenceAsync(string categoryCode)
		{
			var lastSku = await _context.Products
			  .Where(p => p.SKU.StartsWith(categoryCode + "-"))
			  .OrderByDescending(p => p.SKU)
			  .Select(p => p.SKU)
			  .FirstOrDefaultAsync();
			if (string.IsNullOrEmpty(lastSku))
				return 1;
			var parts = lastSku.Split('-');
			return parts.Length > 1 && int.TryParse(parts[1], out var n) ? n + 1 : 1;
		}

		public async Task<IEnumerable<Product>> GetProductsAsync(bool? isActive)
		{
			var query = _context.Products
				.Include(p => p.ProductColors).
				AsQueryable();
			if (isActive.HasValue)
				query = query.Where(p => p.IsActive == isActive.Value);
			return await query
					.OrderByDescending(p => p.CreatedAt)
					.ToListAsync();
		}

		public async Task<IEnumerable<Product>> SearchAsync(string keyword, bool? isActive)
		{
			var query = _context.Products
				.Include(p => p.ProductColors)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				var lowerKeyword = keyword.Trim().ToLower();
				query = query.Where(p => p.Name.ToLower().Contains(lowerKeyword));
			}

			if (isActive.HasValue)
			{
				query = query.Where(p => p.IsActive == isActive.Value);
			}

			return await query
				.OrderByDescending(p => p.CreatedAt)
				.ToListAsync();
		}
	}
}
