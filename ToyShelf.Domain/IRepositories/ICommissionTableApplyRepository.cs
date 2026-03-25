using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface ICommissionTableApplyRepository : IGenericRepository<CommissionTableApply>
	{
		Task<bool> HasOverlapAsync(Guid partnerId, CommissionTableType tableType, DateTime startDate, DateTime? endDate);

		Task<IEnumerable<CommissionTableApply>> GetAllWithDetailsAsync(bool? isActive);

		Task<List<CommissionTableApply>> GetActiveAppliesByPartnerAsync(Guid partnerId, DateTime now);

		Task<List<CommissionTableApply>> GetActiveTierAppliesAsync(Guid partnerId);
	}
}
