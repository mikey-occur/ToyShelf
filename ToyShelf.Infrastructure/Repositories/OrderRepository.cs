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
	public class OrderRepository : GenericRepository<Order>, IOrderRepository
	{
		public OrderRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId)
		{
			return await _context.Orders
				.Include(o => o.Store)
				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.ProductColor)
						.ThenInclude(pc => pc.Product)
				.FirstOrDefaultAsync(o => o.Id == orderId);
		}

		public async Task<Order?> GetOrderWithItemsAndStoreAsync(long orderCode)
		{
			return await _context.Orders
				.Include(o => o.Store)
					.ThenInclude(s => s.Partner)
						.ThenInclude(p => p.PartnerTier)
				.Include(o => o.OrderItems)
				.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
		}
	}
}

