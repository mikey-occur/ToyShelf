using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface ICommissionHistoryRepsitory : IGenericRepository<CommissionHistory>
	{
		Task<List<CommissionHistory>> GetUnsettledHistoriesAsync(DateTime endOfMonth);
		Task<(IEnumerable<CommissionHistory> Items, int TotalCount)> GetHistoriesPaginatedAsync(
			int pageNumber = 1,
			int pageSize = 10,
			Guid? partnerId = null,
			string? searchItem = null, Guid? storeId = null,
			DateTime? fromDate = null,
			DateTime? toDate = null);

        Task<decimal> GetTotalUnsettledAmountAsync(Guid partnerId);
    }
}
