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
	public class ShelfTypeRepository : GenericRepository<ShelfType>, IShelfTypeRepository
	{
		public ShelfTypeRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
		{
			return await _context.ShelfTypes
				.AnyAsync(x => x.Name == name && (!excludeId.HasValue || x.Id != excludeId.Value));
		}

		public async Task<ShelfType?> GetByIdWithLevelsAsync(Guid id)
		{
			return await _context.ShelfTypes
			.Include(x => x.ShelfTypeLevels) 
			.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<IEnumerable<ShelfType>> GetShelfTypesAsync(bool? isActive, string? searchName = null, string? categoryType = null)
		{
			
			var query = _context.ShelfTypes
				.Include(st => st.ShelfTypeLevels) 
				.AsQueryable();

			
			if (isActive.HasValue)
			{
				query = query.Where(st => st.IsActive == isActive.Value);
			}

			
			if (!string.IsNullOrWhiteSpace(searchName))
			{

				var keyword = searchName.ToLower();
				query = query.Where(st => st.Name.ToLower().Contains(keyword));
			}

			// 3. Lọc theo loại sản phẩm đề xuất (Tìm kiếm gần đúng)
			if (!string.IsNullOrWhiteSpace(categoryType))
			{
				
				var categories = categoryType.Split(',')
											 .Select(c => c.Trim())
											 .Where(c => !string.IsNullOrEmpty(c))
											 .ToList();

				if (categories.Any())
				{

					query = query.Where(st => categories.Any(c =>
							   st.SuitableProductCategoryTypes.ToLower().Contains(c.ToLower())
						   ));
				}
			}

			return await query
					.OrderByDescending(st => st.Id) 
					.ToListAsync();
		}
	}
}
