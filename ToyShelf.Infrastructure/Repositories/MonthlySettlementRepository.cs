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
	public class MonthlySettlementRepository : GenericRepository<MonthlySettlement>, IMonthlySettlementRepository
	{
		public MonthlySettlementRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<MonthlySettlement>> GetFilteredSettlementsAsync(int? year, int? month, Guid? partnerId, string? status)
		{
			var query = _context.MonthlySettlements
					.Include(x => x.Partner)
					.AsQueryable();

					if (year.HasValue)
						query = query.Where(x => x.Year == year.Value);

					if (month.HasValue)
						query = query.Where(x => x.Month == month.Value);

					if (partnerId.HasValue && partnerId != Guid.Empty)
						query = query.Where(x => x.PartnerId == partnerId.Value);

					if (!string.IsNullOrWhiteSpace(status))
						query = query.Where(x => x.Status == status);

			return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
		}

		public async Task<MonthlySettlement?> GetSettlementWithDetailsByIdAsync(Guid id)
		{
			return await _context.MonthlySettlements
				.Include(ms => ms.Partner)
				.Include(ms => ms.CommissionHistories) 
				.FirstOrDefaultAsync(ms => ms.Id == id);
		}
	}
}
