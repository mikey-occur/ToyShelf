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
				.Include(o => o.StoreLocation)
					.ThenInclude(sl => sl.Store)
				.Include(o => o.RequestedByUser)
				.Include(o => o.ApprovedByUser)
				.Include(o => o.RejectedByUser)
				.Include(o => o.PartnerAdminApprovedByUser)
				.Include(o => o.AssignmentStoreOrders)
					.ThenInclude(aso => aso.ShipmentAssignment)
						.ThenInclude(sa => sa.Shipper)
				.Include(o => o.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Product)
				.Include(o => o.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Color)
				.AsQueryable();

			if (status.HasValue)
				query = query.Where(x => x.Status == status);

			return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
		}

		public async Task<StoreOrder?> GetByIdWithItemsAsync(Guid id)
		{
			return await _context.StoreOrders
				.Include(o => o.StoreLocation)
					.ThenInclude(sl => sl.Store)
				.Include(o => o.RequestedByUser)
				.Include(o => o.ApprovedByUser)
				.Include(o => o.RejectedByUser)
				.Include(o => o.PartnerAdminApprovedByUser)
				.Include(o => o.AssignmentStoreOrders)
					.ThenInclude(aso => aso.ShipmentAssignment)
						.ThenInclude(sa => sa.Shipper)
				.Include(o => o.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Product)
				.Include(o => o.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Color)
				.FirstOrDefaultAsync(o => o.Id == id);
		}

		public async Task<IEnumerable<StoreOrder>> GetOrdersByPartnerAsync(Guid partnerId, StoreOrderStatus? status)
		{
			var query = _context.StoreOrders
				.Include(o => o.StoreLocation)
					.ThenInclude(sl => sl.Store)
				.Include(o => o.RequestedByUser)
				.Include(o => o.ApprovedByUser)
				.Include(o => o.RejectedByUser)
				.Include(o => o.PartnerAdminApprovedByUser) 
				.Include(o => o.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Product)
				.Include(o => o.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Color)
				.Where(o => o.StoreLocation != null &&
						o.StoreLocation.Store != null &&
						o.StoreLocation.Store.PartnerId == partnerId);

			if (status.HasValue)
			{
				query = query.Where(o => o.Status == status.Value);
			}

			return await query
				.OrderByDescending(o => o.CreatedAt)
				.ToListAsync();
		}
	}
}
