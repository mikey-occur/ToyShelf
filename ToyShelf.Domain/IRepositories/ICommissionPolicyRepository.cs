using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface ICommissionPolicyRepository : IGenericRepository<CommissionPolicy>
	{
		Task<CommissionPolicy?> GetByTierAndSegmentAsync(Guid tierId, Guid segmentId);
		Task<IEnumerable<CommissionPolicy>> GetAllWithDetailsAsync();
		Task<IEnumerable<CommissionPolicy>> GetByTierIdAsync(Guid tierId);
	}
}

