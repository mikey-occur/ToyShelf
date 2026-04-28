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
	public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
	{
		public ShipmentRepository(ToyShelfDbContext context) : base(context) { }

		private IQueryable<Shipment> GetShipmentWithFullDetailsQuery()
		{
			return _context.Shipments
				.Include(x => x.FromLocation)
				.Include(x => x.ToLocation)
				.Include(x => x.Shipper)
				.Include(x => x.RequestedByUser)
				.Include(x => x.Media)

				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.Shipper)

				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.Shipments)

				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentStoreOrders)
						.ThenInclude(aso => aso.StoreOrder)
							.ThenInclude(o => o.Items)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentStoreOrders)
						.ThenInclude(aso => aso.StoreOrder)
							.ThenInclude(o => o.StoreLocation)

				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentShelfOrders)
						.ThenInclude(aso => aso.ShelfOrder)
							.ThenInclude(o => o.Items)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentShelfOrders)
						.ThenInclude(aso => aso.ShelfOrder)
							.ThenInclude(o => o.StoreLocation)

				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentDamageReports)
						.ThenInclude(adr => adr.DamageReport)
							.ThenInclude(dr => dr.Items)
								.ThenInclude(i => i.ProductColor)
									.ThenInclude(pc => pc!.Product)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentDamageReports)
						.ThenInclude(adr => adr.DamageReport)
							.ThenInclude(dr => dr.Items)
								.ThenInclude(i => i.ProductColor)
									.ThenInclude(pc => pc!.Color)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentDamageReports)
						.ThenInclude(adr => adr.DamageReport)
							.ThenInclude(dr => dr.Items)
								.ThenInclude(i => i.Shelf)
									.ThenInclude(s => s!.ShelfType)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.AssignmentDamageReports)
						.ThenInclude(adr => adr.DamageReport)
							.ThenInclude(dr => dr.Items)
								.ThenInclude(i => i.DamageMedia)

				.Include(x => x.Items)
					.ThenInclude(i => i.StoreOrderItem)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc!.Product)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc!.Color)
				.Include(x => x.Items)
					.ThenInclude(i => i.DamageReportItem)
						.ThenInclude(dri => dri!.DamageReport)
				.Include(x => x.Items)
					.ThenInclude(i => i.Shelf)
						.ThenInclude(s => s!.ShelfType)

				.Include(x => x.ShelfShipmentItems)
					.ThenInclude(si => si.ShelfOrderItem)
				.Include(x => x.ShelfShipmentItems)
					.ThenInclude(si => si.Shelf)
						.ThenInclude(sh => sh.ShelfType);
		}
		public async Task<int> GetMaxSequenceAsync()
		{
			var codes = await _context.Shipments
				.Where(x => x.Code.StartsWith("SH-"))
				.Select(x => x.Code)
				.ToListAsync();

			if (!codes.Any()) return 0;

			return codes
				.Select(x => {
					var parts = x.Split('-');
					return parts.Length > 1 && int.TryParse(parts[1], out int val) ? val : 0;
				})
				.Max();
		}

		public async Task<Shipment?> GetByIdWithDetailsAsync(Guid id)
		{
			return await GetShipmentWithFullDetailsQuery()
				.FirstOrDefaultAsync(x => x.Id == id);
		}
		public async Task<List<Shipment>> GetListByAssignmentIdAsync(Guid assignmentId)
		{
			return await GetShipmentWithFullDetailsQuery()
				.Where(x => x.ShipmentAssignmentId == assignmentId)
				.ToListAsync();
		}
		public async Task<IEnumerable<Shipment>> GetAllWithDetailsAsync(ShipmentStatus? shipmentStatus)
		{
			var query = GetShipmentWithFullDetailsQuery();

			if (shipmentStatus.HasValue)
			{
				query = query.Where(x => x.Status == shipmentStatus.Value);
			}

			return await query
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
		}
		public async Task<Shipment?> GetByIdWithItemsAsync(Guid id)
		{
			return await _context.Shipments
				.Include(x => x.FromLocation)
				.Include(x => x.ToLocation)
				.Include(s => s.ShipmentAssignment)
					.ThenInclude(sa => sa.AssignmentStoreOrders)
						.ThenInclude(aso => aso.StoreOrder)
							.ThenInclude(o => o.Items)
				.Include(s => s.ShipmentAssignment)
					.ThenInclude(sa => sa.AssignmentShelfOrders)
						.ThenInclude(aso => aso.ShelfOrder)
							.ThenInclude(o => o.Items)
				.Include(s => s.ShipmentAssignment)
					.ThenInclude(sa => sa.AssignmentDamageReports)
						.ThenInclude(adr => adr.DamageReport)
							.ThenInclude(dr => dr.Items)
				.Include(s => s.Items)
				.Include(s => s.ShelfShipmentItems)
					.ThenInclude(x => x.Shelf).ThenInclude(sh => sh.ShelfType)
				.Include(s => s.ShelfShipmentItems)
					.ThenInclude(x => x.ShelfOrderItem)
				.FirstOrDefaultAsync(s => s.Id == id);
		}

		public async Task<bool> AreAllShipmentsCompletedAsync(Guid assignmentId)
		{
			return !await _context.Shipments
				.AsNoTracking()
				.AnyAsync(s =>
					s.ShipmentAssignmentId == assignmentId &&
					s.Status != ShipmentStatus.Completed);
		}

		public async Task<List<Shipment>> GetByStoreOrderIdAsync(Guid storeOrderId)
		{
			return await GetShipmentWithFullDetailsQuery()
				.Where(x => x.ShipmentAssignment.AssignmentStoreOrders
					.Any(aso => aso.StoreOrderId == storeOrderId))
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
		}

		public async Task<List<Shipment>> GetByShelfOrderIdAsync(Guid shelfOrderId)
		{
			return await GetShipmentWithFullDetailsQuery()
				.Where(x => x.ShipmentAssignment.AssignmentShelfOrders
					.Any(ash => ash.ShelfOrderId == shelfOrderId))
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
		}

		public async Task<List<Shipment>> GetByDamageReportIdAsync(Guid damageReportId)
		{
			return await GetShipmentWithFullDetailsQuery()
				.Where(x => x.ShipmentAssignment.AssignmentDamageReports
					.Any(adr => adr.DamageReportId == damageReportId))
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
		}

		public async Task<Shipment?> GetByIdWithShelfItemsAsync(Guid id)
		{
			return await _context.Shipments
				.Include(s => s.ShelfShipmentItems)
					.ThenInclude(si => si.Shelf)
						.ThenInclude(sh => sh.ShelfType)
				.FirstOrDefaultAsync(s => s.Id == id);
		}

		public async Task<(int TotalDelivering, int TotalCompleted, int TotalCancelled, int TotalAll)> GetShipperStatsAsync(Guid shipperId)
		{
			var stats = await _context.Shipments
			.Where(s => s.ShipperId == shipperId)
			.GroupBy(s => s.ShipperId)
			.Select(g => new
			{
				TotalDelivering = g.Count(s => s.Status == ShipmentStatus.Shipping ||
											   s.Status == ShipmentStatus.ShippingReturn),

				TotalCompleted = g.Count(s => s.Status == ShipmentStatus.Delivered ||
											  s.Status == ShipmentStatus.DeliveredReturn ||
											  s.Status == ShipmentStatus.Completed),

				TotalCancelled = g.Count(s => s.Status == ShipmentStatus.Cancelled),

				TotalAll = g.Count()
			})
			.FirstOrDefaultAsync();

				// Nếu lính mới chưa có đơn, trả về toàn số 0
				if (stats == null)
				{
					return (0, 0, 0, 0);
				}

				return (stats.TotalDelivering, stats.TotalCompleted, stats.TotalCancelled, stats.TotalAll);
		}
	}
	
}
