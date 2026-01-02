using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Store.Request;
using ToyCabin.Application.Models.Store.Response;

namespace ToyCabin.Application.IServices
{
	public interface IStoreService
	{
		// CREATE
		Task<StoreResponse> CreateAsync(CreateStoreRequest request);

		// GET
		Task<IEnumerable<StoreResponse>> GetAllAsync();
		Task<IEnumerable<StoreResponse>> GetActiveAsync();
		Task<IEnumerable<StoreResponse>> GetInactiveAsync();
		Task<StoreResponse> GetByIdAsync(Guid id);

		// UPDATE
		Task<StoreResponse> UpdateAsync(Guid id, UpdateStoreRequest request);

		// STATUS MANAGEMENT
		Task<bool> DisableAsync(Guid id);
		Task<bool> RestoreAsync(Guid id);

		// DELETE
		Task<bool> DeleteAsync(Guid id);
	}
}
