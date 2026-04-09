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

		public async Task<ShipmentAssignment?> GetPendingByLocationAsync(Guid warehouseLocationId, Guid storeLocationId)
		{
			return await _context.ShipmentAssignments
				.Include(x => x.AssignmentStoreOrders).ThenInclude(aso => aso.StoreOrder)
				.Include(x => x.AssignmentShelfOrders).ThenInclude(ash => ash.ShelfOrder)
				.Include(x => x.DamageReports)
				.Where(x => x.WarehouseLocationId == warehouseLocationId
							&& x.Status == AssignmentStatus.Pending
							&& x.ShipperId == null)
				.Where(x =>
					// Check xem trong danh sách đơn hàng có đơn nào thuộc Store này không
					x.AssignmentStoreOrders.Any(aso => aso.StoreOrder.StoreLocationId == storeLocationId) ||
					x.AssignmentShelfOrders.Any(ash => ash.ShelfOrder.StoreLocationId == storeLocationId) ||
					x.DamageReports.Any(dr => dr.InventoryLocationId == storeLocationId)
				)
				.FirstOrDefaultAsync();
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

				// --- Nhánh Shelf Orders (N-N qua bảng trung gian) ---
				.Include(x => x.AssignmentShelfOrders)
					.ThenInclude(ash => ash.ShelfOrder)
						.ThenInclude(o => o.StoreLocation)
				.Include(x => x.AssignmentShelfOrders)
					.ThenInclude(ash => ash.ShelfOrder)
						.ThenInclude(o => o.Items)
							.ThenInclude(i => i.ShelfType)

				// --- Nhánh Damage Reports (1-N) ---
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.InventoryLocation)
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items) 
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc!.Product)
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items)
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc!.Color)
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items)
						.ThenInclude(i => i.Shelf)
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items)
						.ThenInclude(i => i.DamageMedia);
		}
	}
}
