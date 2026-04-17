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
	public class ShelfRepository : GenericRepository<Shelf>, IShelfRepository
	{
		public ShelfRepository(ToyShelfDbContext context) : base(context)
		{
		}

		// ===== PAGINATION =====
		public async Task<(IEnumerable<Shelf> Items, int TotalCount)> GetShelvesPaginatedAsync(
			int pageNumber = 1,
			int pageSize = 10,
			ShelfStatus? status = null,
			Guid? inventoryLocationId = null)
		{
			var query = _context.Shelves
				.Include(s => s.ShelfType)
					.ThenInclude(st => st.ShelfTypeLevels)
				.Include(s => s.InventoryLocation)
				.AsQueryable();

			// Filter
			if (status.HasValue)
				query = query.Where(s => s.Status == status.Value);

			if (inventoryLocationId.HasValue && inventoryLocationId != Guid.Empty)
				query = query.Where(s => s.InventoryLocationId == inventoryLocationId.Value);

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(s => s.AssignedAt)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		// ===== COUNT ACTIVE IN LOCATION =====
		public async Task<int> CountActiveShelvesByLocationAsync(Guid inventoryLocationId)
		{
			return await _context.Shelves
				.CountAsync(s =>
					s.InventoryLocationId == inventoryLocationId &&
					s.Status == ShelfStatus.InUse);
		}

		// ===== GET DETAIL =====
		public async Task<Shelf?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.Shelves
				.Include(s => s.ShelfType)
					.ThenInclude(st => st.ShelfTypeLevels)
				.Include(s => s.InventoryLocation)
				.FirstOrDefaultAsync(s => s.Id == id);
		}

		public async Task<List<Shelf>> GetByIdsWithDetailsAsync(List<Guid> ids)
		{
			return await _context.Shelves
				.Include(s => s.ShelfType)
					.ThenInclude(st => st.ShelfTypeLevels)
				.Include(s => s.InventoryLocation)
					.ThenInclude(l => l.Store)
				.Where(s => ids.Contains(s.Id))
				.ToListAsync();
		}

		public async Task<List<Shelf>> GetAvailableShelvesByType(
			Guid locationId,
			Guid shelfTypeId,
			int quantity)
		{
			return await _context.Shelves
				.Where(s =>
					s.InventoryLocationId == locationId &&
					s.ShelfTypeId == shelfTypeId &&
					s.Status == ShelfStatus.Available)
				.OrderBy(s => s.Code)
				.Take(quantity)
				.ToListAsync();
		}

		public async Task<List<Shelf>> GetByIds(List<Guid> ids)
		{
			if (ids == null || !ids.Any())
				return new List<Shelf>();

			return await _context.Shelves
				.Include(s => s.ShelfType)
				.Include(s => s.InventoryLocation)
				.Where(s => ids.Contains(s.Id))
				.ToListAsync();
		}

		public async Task<string?> GetLastShelfCodeAsync()
		{
			return await _context.Shelves
			.Where(s => s.Code.StartsWith("SH-"))
			.OrderByDescending(s => s.Code)
			.Select(s => s.Code)
			.FirstOrDefaultAsync();
		}
	}
}
