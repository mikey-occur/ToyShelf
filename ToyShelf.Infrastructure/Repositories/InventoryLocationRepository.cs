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
	public class InventoryLocationRepository : GenericRepository<InventoryLocation>, IInventoryLocationRepository
	{
		public InventoryLocationRepository(ToyShelfDbContext context) : base(context) { }
		public async Task<InventoryLocation?> GetByWarehouseIdAsync(Guid warehouseId)
		{
			return await _context.InventoryLocations
				.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId);
		}
		public async Task<InventoryLocation?> GetByStoreIdAsync(Guid storeId)
		{
			return await _context.InventoryLocations
				.FirstOrDefaultAsync(x => x.StoreId == storeId);
		}
		public async Task<List<InventoryLocation>> GetInventoryLocationsAsync(bool? isActive, Guid? StoreId, Guid? WarehouseId)
		{
			var query = _context.InventoryLocations
				.Include(x => x.Inventories)
					.ThenInclude(i => i.ProductColor)
				.Include(x => x.Inventories)
				.Include(x => x.Warehouse)
				.Include(x => x.Store)
				.AsQueryable();

			if (isActive.HasValue)
			{
				query = query.Where(x => x.IsActive == isActive);
			}

			if (StoreId.HasValue)
			{
							query = query.Where(x => x.StoreId == StoreId);
			}

			if (WarehouseId.HasValue)
			{
				query = query.Where(x => x.WarehouseId == WarehouseId);
			}

			return await query.ToListAsync();
		}
	}
}
