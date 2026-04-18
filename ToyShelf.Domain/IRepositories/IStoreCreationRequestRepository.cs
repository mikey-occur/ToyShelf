using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IStoreCreationRequestRepository : IGenericRepository<StoreCreationRequest>
	{
		Task<IEnumerable<StoreCreationRequest>> GetRequestsAsync(StoreRequestStatus? status, Guid? partnerId);
		Task<StoreCreationRequest?> GetByIdWithCityAsync(Guid id);
	}
}
