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

		public async Task<(IEnumerable<CommissionHistory> Items, int TotalCount)> GetHistoriesPaginatedAsync(int pageNumber = 1, int pageSize = 10, Guid? partnerId = null, string? searchItem = null, Guid? storeId = null,
    DateTime? fromDate = null,
    DateTime? toDate = null)
		{
			var query = _context.CommissionHistories.AsQueryable();

			if (partnerId.HasValue)
			{
				query = query.Where(c => c.PartnerId == partnerId.Value);
			}

            if (storeId.HasValue)
                query = query.Where(c => c.OrderItem.Order.StoreId == storeId.Value);

            if (!string.IsNullOrWhiteSpace(searchItem))
			{
				var keyword = searchItem.Trim().ToLower();
				query = query.Where(c =>
					c.OrderItem.Order.CustomerName.ToLower().Contains(keyword) ||
					c.OrderItem.Order.OrderCode.ToString().Contains(keyword)
				);
			}

            if (fromDate.HasValue)
            {
                var utcFrom = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(c => c.CreatedAt >= utcFrom);
            }

            if (toDate.HasValue)
            {
                var utcTo = DateTime.SpecifyKind(toDate.Value.Date, DateTimeKind.Utc);
                var endOfDayTo = utcTo.AddDays(1).AddTicks(-1);
                query = query.Where(c => c.CreatedAt <= endOfDayTo);
            }

            var totalCount = await query.CountAsync();

			if (!string.IsNullOrWhiteSpace(searchItem))
			{
				var keyword = searchItem.Trim().ToLower();
				query = query
					.OrderByDescending(c => c.OrderItem.Order.OrderCode.ToString() == keyword)
					.ThenByDescending(c => c.OrderItem.Order.CustomerName.ToLower().StartsWith(keyword))
					.ThenByDescending(c => c.CreatedAt);
			}
			else
			{
				query = query.OrderByDescending(c => c.CreatedAt);
			}

			var items = await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.Include(c => c.Partner)
				.Include(c => c.OrderItem)
					.ThenInclude(oi => oi.Order) 
				.Include(c => c.MonthlySettlement) 
				.ToListAsync();

			return (items, totalCount);
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
