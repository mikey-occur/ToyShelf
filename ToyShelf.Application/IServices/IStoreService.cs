using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Store.Request;
using ToyShelf.Application.Models.Store.Response;

namespace ToyShelf.Application.IServices
{
	public interface IStoreService
	{
		// CREATE
		Task<StoreResponse> CreateAsync(CreateStoreRequest request);

		// GET
		Task<IEnumerable<StoreResponse>> GetStoresAsync(bool? isActive);
		Task<StoreResponse> GetByIdAsync(Guid id);

		// UPDATE
		Task<StoreResponse> UpdateAsync(Guid id, UpdateStoreRequest request);

		// STATUS MANAGEMENT
		Task DisableAsync(Guid id);
		Task RestoreAsync(Guid id);

		// DELETE
		Task DeleteAsync(Guid id);
	}
}
