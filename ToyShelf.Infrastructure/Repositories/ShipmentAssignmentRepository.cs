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
	public class ShipmentAssignmentRepository : GenericRepository<ShipmentAssignment>, IShipmentAssignmentRepository
	{
		public ShipmentAssignmentRepository(ToyShelfDbContext context) : base(context) { }

		public async Task<ShipmentAssignment?> GetByIdWithDetailsAsync(Guid id)
		{
			return await GetAssignmentWithFullDetailsQuery()
				.FirstOrDefaultAsync(x => x.Id == id);
		}
		public async Task<IEnumerable<ShipmentAssignment>> GetAllWithDetailsAsync()
		{
			return await GetAssignmentWithFullDetailsQuery()
				.ToListAsync();
		}
		public async Task<IEnumerable<ShipmentAssignment>> GetByShipperIdWithOrderAsync(Guid shipperId)
		{
			return await GetAssignmentWithFullDetailsQuery()
				.Where(x => x.ShipperId == shipperId)
				.ToListAsync();
		}
		public async Task<IEnumerable<ShipmentAssignment>> GetByStoreOrderIdWithDetailsAsync(Guid storeOrderId)
		{
			return await GetAssignmentWithFullDetailsQuery()
				.Where(x => x.AssignmentStoreOrders.Any(aso => aso.StoreOrderId == storeOrderId))
				.ToListAsync();
		}

		public async Task<IEnumerable<ShipmentAssignment>> GetByShelfOrderIdWithDetailsAsync(Guid shelfOrderId)
		{
			return await GetAssignmentWithFullDetailsQuery()
				.Where(x => x.AssignmentShelfOrders.Any(ash => ash.ShelfOrderId == shelfOrderId))
				.ToListAsync();
		}

		public async Task<IEnumerable<ShipmentAssignment>> GetByDamageReportIdWithDetailsAsync(Guid damageReportId)
		{
			return await GetAssignmentWithFullDetailsQuery()
				.Where(x => x.AssignmentDamageReports.Any(adr => adr.DamageReportId == damageReportId))
				.ToListAsync();
		}
		public async Task<ShipmentAssignment?> GetPendingByLocationAsync(Guid warehouseLocationId, Guid storeLocationId)
		{
			return await _context.ShipmentAssignments
				.Include(x => x.AssignmentStoreOrders)
					.ThenInclude(aso => aso.StoreOrder)
				.Include(x => x.AssignmentStoreOrders)
					.ThenInclude(aso => aso.AssignmentStoreOrderItems) 
				.Include(x => x.AssignmentShelfOrders)
					.ThenInclude(ash => ash.ShelfOrder)
				.Include(x => x.AssignmentShelfOrders)
					.ThenInclude(ash => ash.AssignmentShelfOrderItems)
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.DamageReport)
				.Where(x => x.WarehouseLocationId == warehouseLocationId
							&& x.Status == AssignmentStatus.Pending
							&& x.ShipperId == null)
				.Where(x =>
					x.AssignmentStoreOrders.Any(aso => aso.StoreOrder.StoreLocationId == storeLocationId) ||
					x.AssignmentShelfOrders.Any(ash => ash.ShelfOrder.StoreLocationId == storeLocationId) ||
					x.AssignmentDamageReports.Any(adr => adr.DamageReport.InventoryLocationId == storeLocationId)
				)
				.FirstOrDefaultAsync();
		}
		/// <summary>
		/// Tính tổng số lượng của một Item cụ thể đã được phân bổ vào các chuyến xe cho một StoreOrder.
		/// Loại trừ các nhiệm vụ đã bị Cancelled.
		/// </summary>
		public async Task<int> GetTotalAllocatedQuantityAsync(Guid storeOrderId, Guid storeOrderItemId)
		{
			return await _context.AssignmentStoreOrderItems
				.AsNoTracking()
				.Where(asoi => asoi.AssignmentStoreOrder.StoreOrderId == storeOrderId
							&& asoi.StoreOrderItemId == storeOrderItemId
							&& asoi.AssignmentStoreOrder.ShipmentAssignment.Status != AssignmentStatus.Cancelled)
				.SumAsync(asoi => asoi.AllocatedQuantity);
		}

		/// <summary>
		/// Tính tổng số lượng của một Item cụ thể đã được phân bổ vào các chuyến xe cho một ShelfOrder.
		/// Loại trừ các nhiệm vụ đã bị Cancelled.
		/// </summary>
		public async Task<int> GetTotalShelfAllocatedQuantityAsync(Guid shelfOrderId, Guid shelfOrderItemId)
		{
			return await _context.AssignmentShelfOrderItems
				.AsNoTracking()
				.Where(ashoi => ashoi.AssignmentShelfOrder.ShelfOrderId == shelfOrderId
							 && ashoi.ShelfOrderItemId == shelfOrderItemId
							 && ashoi.AssignmentShelfOrder.ShipmentAssignment.Status != AssignmentStatus.Cancelled)
				.SumAsync(ashoi => ashoi.AllocatedQuantity);
		}


		public async Task<Dictionary<(Guid locationId, Guid productColorId), int>> GetAllocatedQuantitiesAsync(List<Guid> locationIds, List<Guid> productColorIds)
		{
			var result = await _context.AssignmentStoreOrderItems
				.AsNoTracking()
				.Where(x =>
					locationIds.Contains(x.AssignmentStoreOrder.ShipmentAssignment.WarehouseLocationId) &&
					productColorIds.Contains(x.StoreOrderItem.ProductColorId) &&
					(
						 x.AssignmentStoreOrder.ShipmentAssignment.Status != AssignmentStatus.Cancelled &&
						 x.AssignmentStoreOrder.ShipmentAssignment.Status != AssignmentStatus.Completed
					)
				)
				.GroupBy(x => new
				{
					x.AssignmentStoreOrder.ShipmentAssignment.WarehouseLocationId,
					x.StoreOrderItem.ProductColorId
				})
				.Select(g => new
				{
					LocationId = g.Key.WarehouseLocationId,
					ProductColorId = g.Key.ProductColorId,
					Total = g.Sum(x => x.AllocatedQuantity)
				})
				.ToListAsync();

			return result.ToDictionary(
				x => (x.LocationId, x.ProductColorId),
				x => x.Total
			);
		}

		public async Task<Dictionary<(Guid locationId, Guid shelfTypeId), int>> GetAllocatedShelfQuantitiesAsync(
			List<Guid> locationIds,
			List<Guid> shelfTypeIds)
		{
			var result = await _context.AssignmentShelfOrderItems
				.AsNoTracking()
				.Where(x =>
					locationIds.Contains(x.AssignmentShelfOrder.ShipmentAssignment.WarehouseLocationId) &&
					shelfTypeIds.Contains(x.ShelfOrderItem.ShelfTypeId) &&
					x.AssignmentShelfOrder.ShipmentAssignment.Status != AssignmentStatus.Cancelled &&
					x.AssignmentShelfOrder.ShipmentAssignment.Status != AssignmentStatus.Completed
				)
				.GroupBy(x => new
				{
					x.AssignmentShelfOrder.ShipmentAssignment.WarehouseLocationId,
					x.ShelfOrderItem.ShelfTypeId
				})
				.Select(g => new
				{
					LocationId = g.Key.WarehouseLocationId,
					ShelfTypeId = g.Key.ShelfTypeId,
					Total = g.Sum(x => x.AllocatedQuantity)
				})
				.ToListAsync();

			return result.ToDictionary(
				x => (x.LocationId, x.ShelfTypeId),
				x => x.Total
			);
		}

		// Helper để tái sử dụng các Include cho DamageReport và Order
		private IQueryable<ShipmentAssignment> GetAssignmentWithFullDetailsQuery()
		{
			return _context.ShipmentAssignments
				.Include(x => x.WarehouseLocation)
				.Include(x => x.Shipper)
				.Include(x => x.Shipments)
				.Include(x => x.AssignedByUser)
				.Include(x => x.CreatedByUser)

				// --- Nhánh Store Orders (N-N qua bảng trung gian) ---
				.Include(x => x.AssignmentStoreOrders)
					.ThenInclude(aso => aso.StoreOrder)
						.ThenInclude(o => o.StoreLocation)
				.Include(x => x.AssignmentStoreOrders)
					.ThenInclude(aso => aso.StoreOrder)
						.ThenInclude(o => o.Items)
							.ThenInclude(i => i.ProductColor)
								.ThenInclude(pc => pc.Product)
				.Include(x => x.AssignmentStoreOrders)
					.ThenInclude(aso => aso.StoreOrder)
						.ThenInclude(o => o.Items)
							.ThenInclude(i => i.ProductColor)
								.ThenInclude(pc => pc.Color)
				.Include(x => x.AssignmentStoreOrders)
					.ThenInclude(aso => aso.StoreOrder)
						.ThenInclude(o => o.Items)
							.ThenInclude(i => i.ProductColor)
								.ThenInclude(pc => pc.Inventories)
				.Include(x => x.AssignmentStoreOrders)
					.ThenInclude(aso => aso.AssignmentStoreOrderItems) 
						.ThenInclude(asoi => asoi.StoreOrderItem)
							.ThenInclude(soi => soi.ProductColor)
								.ThenInclude(pc => pc.Product)

				// --- Nhánh Shelf Orders (N-N qua bảng trung gian) ---
				.Include(x => x.AssignmentShelfOrders)
					.ThenInclude(ash => ash.ShelfOrder)
						.ThenInclude(o => o.StoreLocation)
				.Include(x => x.AssignmentShelfOrders)
					.ThenInclude(ash => ash.ShelfOrder)
						.ThenInclude(o => o.Items)
							.ThenInclude(i => i.ShelfType)
				.Include(x => x.AssignmentShelfOrders)
					.ThenInclude(ash => ash.AssignmentShelfOrderItems) 
						.ThenInclude(ashi => ashi.ShelfOrderItem)
							.ThenInclude(shi => shi.ShelfType)

				// --- Nhánh Damage Reports (1-N) ---
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.DamageReport)
						.ThenInclude(dr => dr.InventoryLocation)
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.DamageReport)
						.ThenInclude(dr => dr.Items)
							.ThenInclude(i => i.ProductColor)
								.ThenInclude(pc => pc!.Product)
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.DamageReport)
						.ThenInclude(dr => dr.Items)
							.ThenInclude(i => i.ProductColor)
								.ThenInclude(pc => pc!.Color)
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.DamageReport)
						.ThenInclude(dr => dr.Items)
							.ThenInclude(i => i.Shelf)
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.DamageReport)
						.ThenInclude(dr => dr.Items)
							.ThenInclude(i => i.DamageMedia);
		}
	}
}
