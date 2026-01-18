using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Partner.Request;
using ToyCabin.Application.Models.Partner.Response;

namespace ToyCabin.Application.IServices
{
	public interface IPartnerService
	{
		// ===== CREATE =====
		Task<PartnerResponse> CreateAsync(CreatePartnerRequest request);

		// ===== GET =====
		Task<IEnumerable<PartnerResponse>> GetPartnersAsync(bool? isActive);
		Task<PartnerResponse> GetByIdAsync(Guid id);

		// ===== UPDATE =====
		Task<PartnerResponse> UpdateAsync(Guid id, UpdatePartnerRequest request);

		// ===== DISABLE / RESTORE =====
		Task DisableAsync(Guid id);
		Task RestoreAsync(Guid id);

		// ===== DELETE (hard) =====
		Task DeleteAsync(Guid id);
	}
}
