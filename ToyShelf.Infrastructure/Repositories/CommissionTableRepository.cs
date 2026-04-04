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
	public class CommissionTableRepository : GenericRepository<CommissionTable>, ICommissionTableRepository
	{
		public CommissionTableRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<CommissionTable?> GetActiveByTierTableAsync(Guid tierId)
		{
			return await _context.CommissionTables
			.Include(t => t.PartnerTier)
		.FirstOrDefaultAsync(t => t.Type == CommissionTableType.Tier
							   && t.PartnerTierId == tierId
							   && t.IsActive);
		}

		public async Task<CommissionTable?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.CommissionTables
			.Include(pt => pt.PartnerTier)
			.Include(pt => pt.CommissionItems)
				.ThenInclude(pi => pi.ItemCategories)
				.ThenInclude(ic => ic.ProductCategory)
			.FirstOrDefaultAsync(pt => pt.Id == id);
		}

		public async Task<IEnumerable<CommissionTable>> GetPriceTablesAsync(bool? isActive)
		{
			var query = _context.CommissionTables
			.Include(pt => pt.PartnerTier)
			.Include(pt => pt.CommissionItems)
				.ThenInclude(pi => pi.ItemCategories)
				.ThenInclude(ic => ic.ProductCategory)
			.AsQueryable();

			if (isActive.HasValue)
			{
				query = query.Where(pt => pt.IsActive == isActive.Value);
			}
			return await query
			.OrderBy(pt => pt.Type)
			.ToListAsync();
		}

		public async Task<bool> IsPriceTableInUseAsync(Guid id)
		{
			return await _context.CommissionTableApplies.AnyAsync(x => x.CommissionTableId == id);
		}


	}
}
