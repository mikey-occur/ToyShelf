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
		public async Task<List<InventoryLocation>> GetInventoryLocationsAsync(bool? isActive, Guid? StoreId, Guid? WarehouseId, string? locationType = null, Guid? partnerId = null)
		{
			var query = _context.InventoryLocations
				.Include(x => x.Inventories)
					.ThenInclude(i => i.ProductColor)
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

			if (!string.IsNullOrWhiteSpace(locationType))
			{
				var type = locationType.Trim().ToUpper();
				if (type == "STORE")
				{
					
					query = query.Where(x => x.StoreId != null);
				}
				else if (type == "WAREHOUSE")
				{
					
					query = query.Where(x => x.WarehouseId != null);
				}
			}

			if (partnerId.HasValue)
			{
				query = query.Where(x =>
					x.Store != null && x.Store.PartnerId == partnerId.Value);
			}


			return await query.ToListAsync();
		}

		public async Task<InventoryLocation?> GetStoreLocationByStoreIdAsync(Guid storeId)
		{
			return await _context.InventoryLocations
				.FirstOrDefaultAsync(x => x.StoreId == storeId && x.Type == InventoryLocationType.Store);
		}
		public async Task<List<InventoryLocation>> GetWarehouseLocationsByCityAsync(Guid cityId)
		{
			return await _context.InventoryLocations
				.Include(l => l.Warehouse)
				.Where(l => l.WarehouseId != null && l.Warehouse!.CityId == cityId)
				.ToListAsync();
		}
	}
}
