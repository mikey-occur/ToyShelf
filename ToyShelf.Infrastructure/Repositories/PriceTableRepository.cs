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
	public class PriceTableRepository : GenericRepository<PriceTable>, IPriceTableRepository
	{
		public PriceTableRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<PriceTable?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.PriceTables
			.Include(pt => pt.PartnerTier)           
			.Include(pt => pt.PriceItems)            
				.ThenInclude(pi => pi.PriceSegment)  
			.FirstOrDefaultAsync(pt => pt.Id == id);
		}

		public Task<IEnumerable<PriceTable>> GetPriceTablesAsync(bool? isActive)
		{
			var query = _context.PriceTables.AsQueryable();
			if (isActive.HasValue)
				query = query.Where(pt => pt.IsActive == isActive.Value);
			return Task.FromResult(query.AsEnumerable());
		}

		public async Task<bool> IsPriceTableInUseAsync(Guid id)
		{
			return await _context.PriceTableApplies.AnyAsync(x => x.PriceTableId == id);
		}
	}
}
