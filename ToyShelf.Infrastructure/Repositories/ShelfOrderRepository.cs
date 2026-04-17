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
	public class ShelfOrderRepository : GenericRepository<ShelfOrder>, IShelfOrderRepository
	{
		public ShelfOrderRepository(ToyShelfDbContext context) : base(context){}
		public async Task<int> GetMaxSequenceAsync()
		{
			var codes = await _context.ShelfOrders
				.Select(x => x.Code)
				.ToListAsync();

			if (!codes.Any()) return 0;

			return codes
				.Select(c =>
				{
					var parts = c.Split('-');
					return int.TryParse(parts.Last(), out var num) ? num : 0;
				})
				.Max();
		}

		public async Task<IEnumerable<ShelfOrder>> GetAllWithItemsAsync(ShelfOrderStatus? status)
		{
			var query = _context.ShelfOrders
				.Include(o => o.StoreLocation).ThenInclude(x => x.Store)
				.Include(o => o.RequestedByUser)
				.Include(o => o.ApprovedByUser)
				.Include(o => o.RejectedByUser)
				.Include(o => o.AssignmentShelfOrders)
					.ThenInclude(ash => ash.ShipmentAssignment)
				.Include(o => o.Items)
					.ThenInclude(i => i.ShelfType)

				.AsQueryable();

			if (status.HasValue)
				query = query.Where(x => x.Status == status);

			return await query.ToListAsync();
		}

		public async Task<ShelfOrder?> GetByIdWithItemsAsync(Guid id)
		{
			return await _context.ShelfOrders
				.Include(o => o.StoreLocation).ThenInclude(x => x.Store)
				.Include(o => o.RequestedByUser)
				.Include(o => o.ApprovedByUser)
				.Include(o => o.RejectedByUser)
				.Include(o => o.AssignmentShelfOrders)
					.ThenInclude(ash => ash.ShipmentAssignment)
				.Include(o => o.Items)
					.ThenInclude(i => i.ShelfType)
				.FirstOrDefaultAsync(x => x.Id == id);
		}
	}
}
