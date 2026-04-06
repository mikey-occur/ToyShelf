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
			return await _context.ShipmentAssignments
				.Include(x => x.WarehouseLocation)
				.Include(x => x.Shipper)
				.Include(x => x.Shipments)
				.Include(x => x.AssignedByUser)
				.Include(x => x.CreatedByUser)

				.Include(x => x.StoreOrder)
					.ThenInclude(o => o.StoreLocation)

				.Include(x => x.StoreOrder)
					.ThenInclude(o => o.Items)
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc.Product)

				.Include(x => x.StoreOrder)
					.ThenInclude(o => o.Items)
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc.Color)

				.Include(x => x.ShelfOrder)
					.ThenInclude(o => o.StoreLocation)

				.Include(x => x.ShelfOrder)
					.ThenInclude(o => o.Items)
						.ThenInclude(i => i.ShelfType)

				.Where(x => x.ShipperId == shipperId)
				.ToListAsync();
		}

		public async Task<IEnumerable<ShipmentAssignment>> GetByStoreOrderIdWithDetailsAsync(Guid storeOrderId)
		{
			return await _context.ShipmentAssignments
				.Include(x => x.WarehouseLocation)
				.Include(x => x.CreatedByUser)
				.Include(x => x.Shipper)
				.Include(x => x.Shipments)
				.Include(x => x.AssignedByUser)

				.Include(x => x.StoreOrder)
					.ThenInclude(o => o.StoreLocation)

				.Include(x => x.StoreOrder)
					.ThenInclude(o => o.Items)
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc.Product)

				.Include(x => x.StoreOrder)
					.ThenInclude(o => o.Items)
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc.Color)

				.Include(x => x.ShelfOrder)
					.ThenInclude(o => o.StoreLocation)

				.Include(x => x.ShelfOrder)
					.ThenInclude(o => o.Items)
						.ThenInclude(i => i.ShelfType)

				.Where(x => x.StoreOrderId == storeOrderId)
				.ToListAsync();
		}


		public async Task<ShipmentAssignment?> GetPendingByLocationAsync(Guid warehouseLocationId, Guid storeLocationId)
		{
			return await _context.ShipmentAssignments
				.Where(x => x.WarehouseLocationId == warehouseLocationId && x.Status == AssignmentStatus.Pending && x.ShipperId == null)
				.Where(x =>
					(x.StoreOrder != null && x.StoreOrder.StoreLocationId == storeLocationId) ||
					(x.ShelfOrder != null && x.ShelfOrder.StoreLocationId == storeLocationId) ||
					(x.DamageReport != null && x.DamageReport.InventoryLocationId == storeLocationId)
				)
				.Include(x => x.StoreOrder)
				.Include(x => x.ShelfOrder)
				.Include(x => x.DamageReport)
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
				// Nhánh Store Order
				.Include(x => x.StoreOrder).ThenInclude(o => o!.StoreLocation)
				.Include(x => x.StoreOrder).ThenInclude(o => o!.Items).ThenInclude(i => i.ProductColor).ThenInclude(pc => pc.Product)
				.Include(x => x.StoreOrder).ThenInclude(o => o!.Items).ThenInclude(i => i.ProductColor).ThenInclude(pc => pc.Color)
				// Nhánh Shelf Order
				.Include(x => x.ShelfOrder).ThenInclude(o => o!.StoreLocation)
				.Include(x => x.ShelfOrder).ThenInclude(o => o!.Items).ThenInclude(i => i.ShelfType)
				// NHÁNH DAMAGE REPORT (MỚI BỔ SUNG)
				.Include(x => x.DamageReport).ThenInclude(dr => dr!.InventoryLocation)
				.Include(x => x.DamageReport).ThenInclude(dr => dr!.ProductColor).ThenInclude(pc => pc!.Product)
				.Include(x => x.DamageReport).ThenInclude(dr => dr!.ProductColor).ThenInclude(pc => pc!.Color)
				.Include(x => x.DamageReport).ThenInclude(dr => dr!.Shelf)
				.Include(x => x.DamageReport).ThenInclude(dr => dr!.DamageMedia);
		}
	}
}
