using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Partner.Request;
using ToyShelf.Application.Models.Partner.Response;

namespace ToyShelf.Application.IServices
{
	public interface IPartnerService
	{
		// ===== CREATE =====
		Task<PartnerCreateResponse> CreateAsync(CreatePartnerRequest request);

		// ===== GET =====
		Task<IEnumerable<PartnerResponse>> GetPartnersAsync(bool? isActive);
		Task<PartnerDetailResponse> GetByIdAsync(Guid id);

		// ===== UPDATE =====
		Task<PartnerResponse> UpdateAsync(Guid id, UpdatePartnerRequest request);

		// ===== DISABLE / RESTORE =====
		Task DisableAsync(Guid id);
		Task RestoreAsync(Guid id);

		// ===== DELETE (hard) =====
		Task DeleteAsync(Guid id);
	}
}
