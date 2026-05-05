using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IMonthlySettlementRepository : IGenericRepository<MonthlySettlement>
	{
		Task<MonthlySettlement?> GetSettlementWithDetailsByIdAsync(Guid id);
		Task<IEnumerable<MonthlySettlement>> GetFilteredSettlementsAsync(int? year, int? month, Guid? partnerId, string? status);
        Task<decimal> GetTotalPendingAmountAsync(Guid partnerId);
        Task<List<Order>> GetOrdersBySettlementIdAsync(Guid settlementId);
    }
}
