using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;
using static ToyShelf.Domain.IRepositories.IPartnerRepository;

namespace ToyShelf.Infrastructure.Repositories
{
	public class PartnerRepository : GenericRepository<Partner>, IPartnerRepository
	{
		public PartnerRepository(ToyShelfDbContext context) : base(context) { }
		public async Task<IEnumerable<Partner>> GetPartnerAsync(bool? isActive)
		{
			var query = _context.Partners
								.Include(p => p.PartnerTier)
								.AsQueryable();

			if (isActive.HasValue)
				query = query.Where(s => s.IsActive == isActive.Value);

			return await query
							.OrderByDescending(w => w.CreatedAt)
							.ToListAsync();
		}

		public async Task<Partner?> GetByIdWithTierAsync(Guid id)
		{
			string mainRoleName = "partneradmin";

			return await _context.Partners
				.Include(p => p.PartnerTier)
				.Include(p => p.CommissionTableApplies)
			    .ThenInclude(c => c.CommissionTable)
				.Include(p => p.Users.Where(u =>
					u.Accounts.Any(a => a.AccountRoles.Any(ar =>
						ar.Role.Name.ToLower().Trim() == mainRoleName))))
					.ThenInclude(u => u.Accounts)
						.ThenInclude(a => a.AccountRoles)
							.ThenInclude(ar => ar.Role)
				.FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<IEnumerable<Partner>> GetByCodePrefixAsync(string prefix)
		{
			return await _context.Partners
				.Where(p => p.Code.StartsWith(prefix + "-"))
				.ToListAsync();
		}

		public async Task<(decimal Revenue, int Orders, decimal Commission, int Stores)> GetPartnerStatsByDateAsync(Guid partnerId, DateTime? startDate = null, DateTime? endDate = null)
		{
			var query = _context.CommissionHistories
				.Include(c => c.OrderItem)     
					.ThenInclude(oi => oi.Order)
				.Where(c => c.PartnerId == partnerId)
				.AsQueryable();

			if (startDate.HasValue)
			{
				query = query.Where(c => c.CreatedAt >= startDate.Value);
			}

			if (endDate.HasValue)
			{
				query = query.Where(c => c.CreatedAt <= endDate.Value);
			}

			var revenue = await query.SumAsync(c => (decimal?)c.SalesAmount) ?? 0m;
			var commission = await query.SumAsync(c => (decimal?)c.CommissionAmount) ?? 0m;

			var ordersCount = await query.Select(c => c.OrderItem.OrderId).Distinct().CountAsync();

			var storesCount = await query.Select(c => c.OrderItem.Order.StoreId).Distinct().CountAsync();

			return (revenue, ordersCount, commission, storesCount);
		}


		public async Task<List<PartnerDailyStatResult>> GetPartnerChartDataAsync(Guid partnerId, DateTime startDate, DateTime endDate)
		{
			var query = _context.CommissionHistories
			.Include(c => c.OrderItem)
			.Where(c => c.PartnerId == partnerId
					 && c.CreatedAt >= startDate
					 && c.CreatedAt <= endDate);

				var groupedData = await query
					.GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month, c.CreatedAt.Day })
					.Select(g => new PartnerDailyStatResult
					{
						Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day), 
						TotalRevenue = g.Sum(c => (decimal?)c.SalesAmount) ?? 0m,
						TotalCommission = g.Sum(c => (decimal?)c.CommissionAmount) ?? 0m,
						TotalOrders = g.Select(c => c.OrderItem.OrderId).Distinct().Count()
					})
					.ToListAsync();

			return groupedData;
		}
	}
}
