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
}