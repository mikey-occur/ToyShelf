using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
	public class ProductColorRepository : GenericRepository<ProductColor>, IProductColorRepository
	{
		public ProductColorRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<bool> ExistsBySkuAsync(string sku)
		{
			return await _context.ProductColors.AnyAsync(x => x.Sku == sku && x.IsActive);
		}

		public Task<IEnumerable<ProductColor>> GetProductColorsAsync(bool? isActive)
		{
			var query = _context.ProductColors.AsQueryable();
			if (isActive.HasValue)
				query = query.Where(pc => pc.IsActive == isActive.Value);
			return Task.FromResult(query.AsEnumerable());
		}

		public async Task<ProductColor?> GetColorBySkuAsync(string sku)
		{
			return await _context.ProductColors
				.Include(c => c.Product)
				.Include(c => c.Color)
				.FirstOrDefaultAsync(c => c.Sku == sku);
		}

		public async Task<ProductColor?> GetByIdWithProductAsync(Guid id)
		{
			return await _context.ProductColors
				.Include(pc => pc.Product)
				.FirstOrDefaultAsync(pc => pc.Id == id);
		}

		public async Task<List<ProductColor>> SearchColorsBySkuAsync(string keyword)
		{
			if (string.IsNullOrWhiteSpace(keyword))
				return new List<ProductColor>();

			return await _context.ProductColors
				.Include(c => c.Product)
				.Include(c => c.Color)
				.Where(c => EF.Functions.ILike(c.Sku, $"%{keyword}%"))
				.ToListAsync();
		}
	}
}
