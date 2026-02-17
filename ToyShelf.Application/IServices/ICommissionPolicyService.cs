using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.CommissionPolicy.Request;
using ToyShelf.Application.Models.CommissionPolicy.Response;

namespace ToyShelf.Application.IServices
{
	public interface ICommissionPolicyService
	{
		Task<CommissionPolicyResponse> CreateAsync(CommissionPolicyRequest request);
		Task<CommissionPolicyResponse> UpdateAsync(Guid id, UpdateCommissionPolicyRequest request);
		Task<bool> DeleteAsync(Guid id);
		Task<CommissionPolicyResponse> GetByIdAsync(Guid id);
		Task<IEnumerable<CommissionPolicyResponse>> GetAllAsync();
		Task<IEnumerable<CommissionPolicyResponse>> GetByTierIdAsync(Guid tierId);
	}
}
