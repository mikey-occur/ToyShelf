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
	public class InventoryShelfRepository : GenericRepository<InventoryShelf>, IInventoryShelfRepository
	{
		public InventoryShelfRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<List<InventoryShelf>> GetDistributionsByShelfTypeAsync(Guid shelfTypeId)
		{
			return await _context.InventoryShelves
				.Where(x => x.ShelfTypeId == shelfTypeId)
				.Include(x => x.InventoryLocation) 
				.Include(x => x.ShelfType)         
				.ToListAsync();
		
		}

		public async Task<InventoryShelf?> GetShelfAsync(Guid locationId, Guid shelfTypeId)
		{
			return await _context.InventoryShelves
				 .FirstOrDefaultAsync(x => x.InventoryLocationId == locationId && x.ShelfTypeId == shelfTypeId);
		}

		public async Task<List<InventoryShelf>> GetShelvesByLocationAsync(Guid locationId)
		{
			return await _context.InventoryShelves
				.Where(x => x.InventoryLocationId == locationId)
				.Include(x => x.ShelfType)
				.Include(x => x.InventoryLocation)
				.ToListAsync();
		}
		public async Task<InventoryShelf?> GetByLocationAndTypeAsync(Guid locationId, Guid shelfTypeId)
		{
			return await _context.InventoryShelves
				.Include(x => x.ShelfType) 
				.FirstOrDefaultAsync(x => x.InventoryLocationId == locationId
									   && x.ShelfTypeId == shelfTypeId);
		}

		public async Task<InventoryShelf?> GetShelfWithStatusAsync(Guid locationId, Guid shelfTypeId, ShelfStatus status)
		{
			return await _context.InventoryShelves
				.Include(x => x.ShelfType) // Include để lấy Name hiển thị lỗi nếu cần
				.FirstOrDefaultAsync(x => x.InventoryLocationId == locationId
									   && x.ShelfTypeId == shelfTypeId
									   && x.Status == status);
		}
	}
}
