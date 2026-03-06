using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IPriceTableApplyRepository : IGenericRepository<PriceTableApply>
	{
		Task<bool> HasOverlapAsync(Guid partnerId, DateTime startDate, DateTime? endDate);

		Task<IEnumerable<PriceTableApply>> GetAllWithDetailsAsync(bool? isActive);

		Task<PriceTableApply?> GetActiveByPartnerAsync(Guid partnerId, DateTime now);
	}
}
