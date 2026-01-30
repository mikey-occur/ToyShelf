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
	public class ProductCategoryRepository : GenericRepository<ProductCategory>, IProductCategoryRepository
	{
		public ProductCategoryRepository(ToyCabinDbContext context) : base(context)
		{
		}

		public async Task<bool> ExistsCodeAsync(string code, Guid? parentId)
		{
			return await _context.ProductCategories.AnyAsync(x =>x.Code == code && x.ParentId == parentId);
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

		public async Task<bool> HasChildAsync(Guid parentId)
		{
			return await _context.ProductCategories.AnyAsync(x => x.ParentId == parentId);
		}
	}
}
