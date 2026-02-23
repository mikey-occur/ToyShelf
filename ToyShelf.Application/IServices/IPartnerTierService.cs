using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.PartnerTier.Request;
using ToyShelf.Application.Models.PartnerTier.Response;

namespace ToyShelf.Application.IServices
{
	public interface IPartnerTierService
	{
		Task<IEnumerable<PartnerTierResponse>> GetAllAsync();
		Task<PartnerTierResponse> GetByIdAsync(Guid id);
		Task<PartnerTierResponse> CreateAsync(PartnerTierRequest request);
		Task<PartnerTierResponse> UpdateAsync(Guid id, PartnerTierRequest request);
		Task<bool> DeleteAsync(Guid id);
	}
}
