using Microsoft.EntityFrameworkCore;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;
using ToyShelf.Infrastructure.Repositories;

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
	public InventoryRepository(ToyShelfDbContext context) : base(context)
	{
	}

	public async Task<Inventory?> GetInventoryAsync(Guid storeId, Guid productColorId, string dispositionCode)
	{
		return await _context.Inventories
			.Include(i => i.InventoryLocation)
			.Include(i => i.Disposition)
			.FirstOrDefaultAsync(i =>
				i.ProductColorId == productColorId &&
				i.InventoryLocation.StoreId == storeId &&
				i.Disposition.Code == dispositionCode);
	}
	public async Task<Inventory?> GetAsync(
			Guid locationId,
			Guid productColorId,
			Guid dispositionId)
	{
		return await _context.Inventories
			.FirstOrDefaultAsync(x =>
				x.InventoryLocationId == locationId &&
				x.ProductColorId == productColorId &&
				x.DispositionId == dispositionId);
	}
	public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
	{
		return await _context.Inventories
			.Include(x => x.ProductColor)
			.Include(x => x.InventoryLocation)
			.Include(x => x.Disposition)
			.ToListAsync();
	}

	public async Task<IEnumerable<Inventory>> GetByLocationAsync(Guid locationId)
	{
		return await _context.Inventories
			.Where(x => x.InventoryLocationId == locationId)
			.Include(x => x.ProductColor)
			.Include(x => x.InventoryLocation)
			.Include(x => x.Disposition)
			.ToListAsync();
	}

}