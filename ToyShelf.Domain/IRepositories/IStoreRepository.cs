using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IStoreRepository : IGenericRepository<Store>
	{
		Task<IEnumerable<Store>> GetStoresAsync(
			bool? isActive,
			Guid? ownerId = null,
			string? keyword = null,
			Guid? cityId = null);
		Task<Store?> GetByIdWithDetailsAsync(Guid id);
		Task<int> GetMaxSequenceByPartnerAsync(Guid partnerId);
		Task<bool> ExistsByCodeInPartnerAsync(string code, Guid partnerId);
	}
}
