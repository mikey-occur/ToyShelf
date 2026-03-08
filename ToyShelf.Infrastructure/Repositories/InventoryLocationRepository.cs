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
	}
}
