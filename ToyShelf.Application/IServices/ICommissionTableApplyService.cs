using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.PriceTableApply.Request;
using ToyShelf.Application.Models.PriceTableApply.Response;

namespace ToyShelf.Application.IServices
{
	public interface ICommissionTableApplyService
	{
		Task<CommissionTableApplyResponse> CreateAsync(CommissionTableApply request);
		Task<IEnumerable<CommissionTableApplyResponse>> GetAllAsync(bool? isActive);
		Task<CommissionTableApplyResponse> UpgradePartnerTierAsync(Guid partnerId, Guid newTierId);
		Task<bool> DeleteAsync(Guid id);
		Task<bool> RestorePriceTableApplyAsync(Guid id);
		Task<bool> DisablePriceTableApply(Guid id);
	}
}
