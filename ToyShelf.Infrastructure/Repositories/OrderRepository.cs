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

		public async Task<List<Order>> GetOrdersAsync(Guid? storeId, Guid? partnerId, string? phone)
		{
			
			var query = _context.Orders
				.Include(o => o.Store)
				.Include(o => o.Staff)
				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.ProductColor) 
				.AsQueryable();

			if (storeId.HasValue)
			{
				query = query.Where(o => o.StoreId == storeId.Value);
			}

			if (partnerId.HasValue)
			{
				query = query.Where(o => o.Store != null && o.Store.PartnerId == partnerId.Value);
			}

			if (!string.IsNullOrWhiteSpace(phone))
			{
				var cleanPhone = phone.Trim();
				query = query.Where(o => o.CustomerPhone.Contains(cleanPhone));
			}

			return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
		}

		public async Task<IEnumerable<Order>> GetOrdersByCustomerPhoneAsync(string phone)
		{
			return await _context.Orders
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.ProductColor) 
				.Include(o => o.Store)
				.Where(o => o.CustomerPhone.Contains(phone)) 
				.OrderByDescending(o => o.CreatedAt) 
				.ToListAsync();
		}

		public async Task<Order?> GetOrderWithDetailsByCodeAsync(long orderCode)
		{
			return await _context.Orders
				.Include(o => o.Store)
				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.ProductColor)
						.ThenInclude(pc => pc.Product)
				.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
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

		public async Task<(int TotalOrders, decimal TotalRevenue)> GetStoreStatsAsync(Guid storeId, DateTime? fromDate = null, DateTime? toDate = null)
		{
			var query = _context.Orders
		.Where(o => o.StoreId == storeId && o.Status.ToUpper() == "PAID")
		.AsQueryable();

		
			if (fromDate.HasValue)
			{
				query = query.Where(o => o.CreatedAt >= fromDate.Value);
			}

			
			if (toDate.HasValue)
			{
				query = query.Where(o => o.CreatedAt <= toDate.Value);
			}

			
			var totalOrders = await query.CountAsync();
			var totalRevenue = await query.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

			return (totalOrders, totalRevenue);
		}
	}
}

