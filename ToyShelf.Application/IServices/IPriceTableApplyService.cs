using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.PriceTableApply.Request;
using ToyShelf.Application.Models.PriceTableApply.Response;

namespace ToyShelf.Application.IServices
{
	public interface IPriceTableApplyService
	{
		Task<PriceTableApplyResponse> CreateAsync(PriceTableApplyRequest request);
		Task<IEnumerable<PriceTableApplyResponse>> GetAllAsync(bool? isActive);
		Task<bool> DeleteAsync(Guid id);
		Task<bool> RestorePriceTableApplyAsync(Guid id);
		Task<bool> DisablePriceTableApply(Guid id);
	}
}
