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

		public async Task<ShipmentAssignment?> GetByIdWithDetailsAsync(Guid id)
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

				.FirstOrDefaultAsync(x => x.Id == id);
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

		public async Task<IEnumerable<ShipmentAssignment>> GetAllWithDetailsAsync()
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

				.ToListAsync();
		}
	}
}
