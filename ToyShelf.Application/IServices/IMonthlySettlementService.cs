using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.MonthlySettlement.Response;

namespace ToyShelf.Application.IServices
{
	public interface IMonthlySettlementService
	{
		Task<List<MonthlySettlementResponse>> GenerateMonthlySettlementAsync(int month, int year);
		Task<MonthlySettlementResponse?> GetByIdAsync(Guid id);
		Task<bool> PayAsync(Guid id);
		Task<IEnumerable<MonthlySettlementResponse>> GetAllAsync();
	}
}

