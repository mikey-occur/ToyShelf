using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IPartnerRepository : IGenericRepository<Partner>
	{
		Task<IEnumerable<Partner>> GetPartnerAsync(bool? isActive);
		Task<Partner?> GetByIdWithTierAsync(Guid id);
		Task<IEnumerable<Partner>> GetByCodePrefixAsync(string prefix);

		Task<(decimal Revenue, int Orders, decimal Commission, int Stores)> GetPartnerStatsByDateAsync(
			Guid partnerId,
			DateTime? startDate = null,
			DateTime? endDate = null);

		Task<List<PartnerDailyStatResult>> GetPartnerChartDataAsync(Guid partnerId, DateTime startDate, DateTime endDate);

		public class PartnerDailyStatResult
		{
			public DateTime Date { get; set; }
			public int TotalOrders { get; set; }
			public decimal TotalRevenue { get; set; }
			public decimal TotalCommission { get; set; }
		}
	}
}
