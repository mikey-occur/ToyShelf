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

		public async Task<MonthlySettlement?> GetSettlementWithDetailsByIdAsync(Guid id)
		{
			return await _context.MonthlySettlements
				.Include(ms => ms.Partner)
				.Include(ms => ms.CommissionHistories) 
				.FirstOrDefaultAsync(ms => ms.Id == id);
		}
	}
}
