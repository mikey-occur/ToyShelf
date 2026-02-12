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
			var query = _context.Warehouses.AsQueryable();

			if (isActive.HasValue)
				query = query.Where(s => s.IsActive == isActive.Value);

			return await query
					.OrderByDescending(w => w.CreatedAt)
					.ToListAsync();
		}
	}
}
