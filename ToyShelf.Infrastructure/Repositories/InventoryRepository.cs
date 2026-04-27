using Microsoft.EntityFrameworkCore;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;
using ToyShelf.Infrastructure.Repositories;

public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
	public InventoryRepository(ToyShelfDbContext context) : base(context){}
	public async Task<Inventory?> GetInventoryAsync(
		Guid storeId,
		Guid productColorId,
		InventoryStatus status)
		{
			return await _context.Inventories
				.Include(i => i.InventoryLocation)
				.FirstOrDefaultAsync(i =>
					i.ProductColorId == productColorId &&
					i.InventoryLocation.StoreId == storeId &&
					i.Status == status);
		}

	public async Task<Inventory?> GetAsync(
		Guid locationId,
		Guid productColorId,
		InventoryStatus status)
		{
			return await _context.Inventories
				.FirstOrDefaultAsync(x =>
					x.InventoryLocationId == locationId &&
					x.ProductColorId == productColorId &&
					x.Status == status);
		}

	public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
		{
			return await _context.Inventories
				.Include(x => x.ProductColor)
				.Include(x => x.InventoryLocation)
				.ToListAsync();
		}

	public async Task<IEnumerable<Inventory>> GetByLocationAsync(Guid locationId)
		{
			return await _context.Inventories
				  .Where(x => x.InventoryLocationId == locationId)
					.Include(x => x.ProductColor)
						.ThenInclude(pc => pc.Product)
							.ThenInclude(p => p.ProductCategory)
					.Include(x => x.ProductColor)
						.ThenInclude(pc => pc.Color)
					.Include(x => x.InventoryLocation)
						.ThenInclude(l => l.Warehouse)
					.Include(x => x.InventoryLocation)
						.ThenInclude(l => l.Store)
					.ToListAsync();
	}
	public async Task<Inventory?> GetByLocationAndProductAsync(Guid locationId, Guid productColorId)
	{
		return await GetByLocationAndProductAsync(locationId, productColorId, InventoryStatus.Available);
	}

	public async Task<Inventory?> GetByLocationAndProductAsync(Guid locationId, Guid productColorId, InventoryStatus status)
	{
		return await _context.Inventories
			.FirstOrDefaultAsync(x =>
				x.InventoryLocationId == locationId &&
				x.ProductColorId == productColorId &&
				x.Status == status);
	}
	public async Task<List<Inventory>> GetByWarehouseIdAsync(Guid warehouseId)
	{
		return await _context.Inventories
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Product)
					.ThenInclude(p => p.ProductCategory)
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Color)
			.Include(i => i.InventoryLocation)
				.ThenInclude(l => l.Warehouse)
			.Where(i => i.InventoryLocation.WarehouseId == warehouseId
					 && i.Status == InventoryStatus.Available)
			.ToListAsync();
	}

	public async Task<List<Inventory>> GetAllByWarehouseIdAsync(Guid warehouseId)
	{
		return await _context.Inventories
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Product)
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Color)
			.Include(i => i.InventoryLocation)
				.ThenInclude(l => l.Warehouse)
			.Where(i => i.InventoryLocation.WarehouseId == warehouseId)
			.ToListAsync();
	}
	public async Task<IEnumerable<Inventory>> GetAllInventoryWithDetailsAsync(InventoryLocationType? type)
	{
		var query = _context.Inventories
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Product)
					.ThenInclude(p => p.ProductCategory)
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Color)
			.Include(i => i.InventoryLocation)
				.ThenInclude(l => l.Warehouse)
			.AsQueryable();

		// Nếu có filter type, áp dụng
		if (type.HasValue)
		{
			query = query.Where(i => i.InventoryLocation != null && i.InventoryLocation.Type == type.Value);
		}

		return await query.ToListAsync();
	}

	public async Task<InventoryLocation?> GetLocationByIdAsync(Guid locationId)
	{
		return await _context.InventoryLocations
			.Include(l => l.Warehouse)
			.Include(l => l.Store)
			.FirstOrDefaultAsync(l => l.Id == locationId);
	}

	public async Task<IEnumerable<Inventory>> GetByProductIdAsync(Guid productId)
	{
		return await _context.Inventories
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Product)
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Color)
			.Include(i => i.InventoryLocation)
				.ThenInclude(l => l.Warehouse)
			.Include(i => i.InventoryLocation)
				.ThenInclude(l => l.Store)
			.Where(i => i.ProductColor != null && i.ProductColor.ProductId == productId)
			.ToListAsync();
	}
	public async Task<List<Inventory>> GetByLocationIdsAsync(List<Guid> locationIds)
	{
		return await _context.Inventories
			.Include(i => i.InventoryLocation)
				.ThenInclude(l => l.Warehouse)
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Product)
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Color)
			.Where(i => locationIds.Contains(i.InventoryLocationId))
			.ToListAsync();
	}
	public async Task<List<Inventory>> GetByWarehouseAndProductIdsAsync(
	Guid warehouseId,
	List<Guid> productIds)
	{
		if (productIds == null || !productIds.Any())
			return new List<Inventory>();

		return await _context.Inventories
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Product)
					.ThenInclude(p => p.ProductCategory)
			.Include(i => i.ProductColor)
				.ThenInclude(pc => pc.Color)
			.Include(i => i.InventoryLocation)
				.ThenInclude(l => l.Warehouse)
			.Where(i =>
				i.InventoryLocation.WarehouseId == warehouseId &&
				productIds.Contains(i.ProductColor.ProductId)
			)
			.ToListAsync();
	}

	public async Task<(int TotalShelves, int TotalProducts)> GetStoreInventoryStatsAsync(Guid storeId)
	{
		// Total Shelves
		var totalShelves = await _context.InventoryShelves
			.Where(s => s.InventoryLocation.StoreId == storeId
				&& s.InventoryLocation.Type == InventoryLocationType.Store)
			.SumAsync(s => (int?)s.Quantity) ?? 0;

		// Total Products
		var totalProducts = await _context.Inventories
			.Where(i => i.InventoryLocation.StoreId == storeId
				&& i.InventoryLocation.Type == InventoryLocationType.Store
				&& i.Status == InventoryStatus.Available)
			.SumAsync(i => (int?)i.Quantity) ?? 0;

		return (totalShelves, totalProducts);
	}
}