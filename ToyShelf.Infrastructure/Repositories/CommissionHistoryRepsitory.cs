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
	public class CommissionHistoryRepsitory : GenericRepository<CommissionHistory>, ICommissionHistoryRepsitory
	{
		public CommissionHistoryRepsitory(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<List<CommissionHistory>> GetUnsettledHistoriesAsync(DateTime endOfMonth)
		{
			return await _context.CommissionHistories
				.Include(ch => ch.Partner)
				.Include(ch => ch.OrderItem)
				.Where(ch => ch.MonthlySettlementId == null && ch.CreatedAt < endOfMonth)
				.ToListAsync();
		}
	}
}
