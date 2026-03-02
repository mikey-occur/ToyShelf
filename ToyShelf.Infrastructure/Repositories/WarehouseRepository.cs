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
	public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
	{
		public WarehouseRepository(ToyShelfDbContext context) : base(context) { }	
		public async Task<IEnumerable<Warehouse>> GetWarehousesAsync(bool? isActive)
		{
			var query = _context.Warehouses
								.Include(w => w.City)
								.AsQueryable();

			if (isActive.HasValue)
				query = query.Where(s => s.IsActive == isActive.Value);

			return await query
					.OrderByDescending(w => w.CreatedAt)
					.ToListAsync();
		}
		public async Task<Warehouse?> GetByIdWithCityAsync(Guid id)
		{
			return await _context.Warehouses
				.Include(w => w.City)
				.FirstOrDefaultAsync(w => w.Id == id);
		}

		public async Task<int> CountByCityAsync(Guid cityId)
		{
			return await _context.Warehouses
				.CountAsync(w => w.CityId == cityId);
		}

		public async Task<bool> ExistsByCodeInCityAsync(string code, Guid cityId)
		{
			return await _context.Warehouses
				.AnyAsync(w => w.CityId == cityId && w.Code == code);
		}

		public async Task<int> GetMaxSequenceByCityAsync(Guid cityId)
		{
			var codes = await _context.Warehouses
				.Where(w => w.CityId == cityId)
				.Select(w => w.Code)
				.ToListAsync();

			if (!codes.Any())
				return 0;

			int max = 0;

			foreach (var code in codes)
			{
				var parts = code.Split('-');

				if (parts.Length < 3) // WH-HCM-01 => 3 phần
					continue;

				if (int.TryParse(parts.Last(), out int number))
				{
					if (number > max)
						max = number;
				}
			}

			return max;
		}
	}
}
