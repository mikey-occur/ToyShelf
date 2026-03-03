using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Infrastructure.Repositories
{
	public class PriceTableApplyRepository : GenericRepository<PriceTableApply>, IPriceTableApplyRepository
	{
		public PriceTableApplyRepository(Context.ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<PriceTableApply>> GetAllWithDetailsAsync(bool? isActive)
		{
			var query = _context.PriceTableApplies
				.Include(x => x.Partner)
				.Include(x => x.PriceTable)
				.AsQueryable();

			if (isActive.HasValue)
			{
				query = query.Where(x => x.IsActive == isActive.Value);
			}

			return await query
				.OrderByDescending(x => x.StartDate)
				.ToListAsync();
		}

		public async Task<bool> HasOverlapAsync(Guid partnerId, DateTime startDate, DateTime? endDate)
		{
		
			var checkEndDate = endDate ?? DateTime.MaxValue;

			
			return await _context.PriceTableApplies.AnyAsync(x =>
				x.PartnerId == partnerId &&
				x.IsActive &&
				x.StartDate < checkEndDate &&
				(x.EndDate ?? DateTime.MaxValue) > startDate
			);
		}
	}
}
