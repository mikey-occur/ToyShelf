using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.CommissionHistory.Response;

namespace ToyShelf.Application.IServices
{
	public interface ICommissionHistoryService
	{
		Task<(IEnumerable<CommissionHistoryResponse> Items, int TotalCount)> GetHistoriesPaginatedAsync(
			int pageNumber = 1,
			int pageSize = 10,
			Guid? partnerId = null,
			string? searchItem = null);
	}
}
