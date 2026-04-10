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
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a!.Shipper)
				// Nhánh sản phẩm
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Product)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Color)
				// Nhánh kệ
				.Include(x => x.ShelfShipmentItems)
					.ThenInclude(si => si.Shelf)
						.ThenInclude(sh => sh.ShelfType)
				// Nhánh Damage Report
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items) // Chui vào danh sách món hỏng
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc!.Product)
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items)
						.ThenInclude(i => i.ProductColor)
							.ThenInclude(pc => pc!.Color)
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items)
						.ThenInclude(i => i.Shelf)
							.ThenInclude(s => s!.ShelfType)
				.Include(x => x.DamageReports)
					.ThenInclude(dr => dr.Items)
						.ThenInclude(i => i.DamageMedia)
				// Nhánh đơn hàng Store và Shelf
				.Include(x => x.StoreOrders)
					.ThenInclude(o => o.StoreLocation)
				.Include(x => x.ShelfOrders)
					.ThenInclude(o => o.StoreLocation);
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

			return await query.ToListAsync();
		}
		public async Task<Shipment?> GetByIdWithItemsAsync(Guid id)
		{
			return await _context.Shipments
				.Include(x => x.FromLocation)
				.Include(x => x.ToLocation)
				.Include(s => s.StoreOrders).ThenInclude(o => o.Items)
				.Include(s => s.ShelfOrders).ThenInclude(o => o.Items)
				.Include(s => s.DamageReports).ThenInclude(dr => dr.Items)
				.Include(s => s.Items)
				.Include(s => s.ShelfShipmentItems)
					.ThenInclude(x => x.Shelf).ThenInclude(sh => sh.ShelfType)
				.FirstOrDefaultAsync(s => s.Id == id);
		}


		public async Task<List<Shipment>> GetByStoreOrderIdAsync(Guid storeOrderId)
		{
			return await GetShipmentWithFullDetailsQuery()
				.Where(x => x.StoreOrders.Any(o => o.Id == storeOrderId))
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
	}
}
