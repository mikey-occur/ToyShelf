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
		public async Task<int> GetMaxSequenceAsync()
		{
			var codes = await _context.Shipments
				.Where(x => x.Code.StartsWith("SH-"))
				.Select(x => x.Code)
				.ToListAsync();

			if (!codes.Any())
				return 0;

			return codes
				.Select(x => int.Parse(x.Split('-')[1]))
				.Max();
		}

		public async Task<Shipment?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.Shipments
				.Include(x => x.StoreOrder)
					.ThenInclude(o => o.StoreLocation)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a.Shipper)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Product)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Color)
				.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<List<Shipment>> GetListByAssignmentIdAsync(Guid assignmentId)
		{
			return await _context.Shipments
				.Include(x => x.FromLocation)
				.Include(x => x.ToLocation)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a.Shipper)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Product)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Color)
				.Where(x => x.ShipmentAssignmentId == assignmentId)
				.ToListAsync();
		}

		public async Task<IEnumerable<Shipment>> GetAllWithDetailsAsync(ShipmentStatus? shipmentStatus)
		{
			var query = _context.Shipments
				.Include(x => x.FromLocation)
				.Include(x => x.ToLocation)
				.Include(x => x.ShipmentAssignment)
					.ThenInclude(a => a.Shipper)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Product)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc.Color)
				.AsQueryable();

			if (shipmentStatus.HasValue)
			{
				query = query.Where(x => x.Status == shipmentStatus.Value);
			}

			return await query.ToListAsync();
		}
		public async Task<Shipment?> GetByIdWithItemsAsync(Guid id)
		{
			return await _context.Shipments
				.Include(s => s.Items)
				.Include(s => s.StoreOrder)
					.ThenInclude(o => o.Items)
				.Include(s => s.StoreOrder)
					.ThenInclude(o => o.Items)
						.ThenInclude(i => i.ProductColor)
				.FirstOrDefaultAsync(s => s.Id == id);
		}
	} 
}
