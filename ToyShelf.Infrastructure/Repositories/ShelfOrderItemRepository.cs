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
	public class ShelfOrderItemRepository : GenericRepository<ShelfOrderItem>, IShelfOrderItemRepository
	{
		public ShelfOrderItemRepository(ToyShelfDbContext context) : base(context) { }

		public async Task<IEnumerable<ShelfOrderItem>> GetByShelfOrderIdAsync(Guid shelfOrderId)
		{
			return await _context.ShelfOrderItems
				.Include(x => x.ShelfType)
				.Where(x => x.ShelfOrderId == shelfOrderId)
				.ToListAsync();
		}
	}
}
