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
	public class StoreOrderRepository : GenericRepository<StoreOrder>, IStoreOrderRepository
	{
		public StoreOrderRepository(ToyShelfDbContext context) : base(context) { }

		public async Task<int> GetMaxSequenceAsync()
		{
			var codes = await _context.StoreOrders
				.Select(x => x.Code)
				.ToListAsync();

			if (!codes.Any())
				return 0;

			return codes
				.Select(c =>
				{
					var parts = c.Split('-');
					return int.TryParse(parts.Last(), out var number) ? number : 0;
				})
				.Max();
		}
		public async Task<IEnumerable<StoreOrder>> GetAllWithItemsAsync(StoreOrderStatus? status)
		{
			var query = _context.StoreOrders
				.Include(o => o.Items)
				.AsQueryable();

			if (status.HasValue) query = query.Where(x => x.Status == status);

			return await query.ToListAsync();
		}


		public async Task<StoreOrder?> GetByIdWithItemsAsync(Guid id)
		{
			return await _context.StoreOrders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == id);
		}
	}
}
