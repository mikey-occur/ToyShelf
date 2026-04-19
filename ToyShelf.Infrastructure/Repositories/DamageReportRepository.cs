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
	public class DamageReportRepository : GenericRepository<DamageReport>, IDamageReportRepository
	{
		public DamageReportRepository(ToyShelfDbContext context) : base(context){}

		public async Task<int> GetMaxSequenceAsync()
		{
			var codes = await _context.DamageReports
				.Select(x => x.Code)
				.ToListAsync();

			if (!codes.Any())
				return 0;

			return codes
				.Select(c =>
				{
					var parts = c.Split('-');
					// DR-yyyyMMdd-XXXX -> Lấy phần cuối cùng (XXXX)
					return int.TryParse(parts.Last(), out var number) ? number : 0;
				})
				.Max();
		}

		public async Task<IEnumerable<DamageReport>> GetAllWithIncludeAsync(DamageStatus? status)
		{
			var query = _context.DamageReports
				.Include(x => x.InventoryLocation)
					.ThenInclude(l => l.Store)
				.Include(x => x.ReportedByUser)
				.Include(x => x.ReviewedByUser)
				.Include(x => x.PartnerAdminApprovedByUser)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc!.Product)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc!.Color)
				.Include(x => x.Items)
					.ThenInclude(i => i.Shelf)
						.ThenInclude(s => s!.ShelfType)
				.Include(x => x.Items)
					.ThenInclude(i => i.DamageMedia)
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.ShipmentAssignment)
						.ThenInclude(sa => sa.Shipments)
				.AsQueryable();

			if (status.HasValue)
				query = query.Where(x => x.Status == status.Value);

			return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
		}

		public async Task<DamageReport?> GetByIdFullIncludeAsync(Guid id)
		{
			return await _context.DamageReports
				.Include(x => x.InventoryLocation)
					.ThenInclude(l => l.Store)
				.Include(x => x.ReportedByUser)
				.Include(x => x.ReviewedByUser)
				.Include(x => x.PartnerAdminApprovedByUser)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc!.Product)
				.Include(x => x.Items)
					.ThenInclude(i => i.ProductColor)
						.ThenInclude(pc => pc!.Color)
				.Include(x => x.Items)
					.ThenInclude(i => i.Shelf)
						.ThenInclude(s => s!.ShelfType)
				.Include(x => x.Items)
					.ThenInclude(i => i.DamageMedia)
				.Include(x => x.AssignmentDamageReports)
					.ThenInclude(adr => adr.ShipmentAssignment)
						.ThenInclude(sa => sa.Shipments)
				.FirstOrDefaultAsync(x => x.Id == id);
		}
	}
}
