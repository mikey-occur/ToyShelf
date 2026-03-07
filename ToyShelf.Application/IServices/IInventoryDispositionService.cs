using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.InventoryDisposition.Request;
using ToyShelf.Application.Models.InventoryDisposition.Response;

namespace ToyShelf.Application.IServices
{
	public interface IInventoryDispositionService
	{
		Task<InventoryDispositionResponse> CreateAsync(CreateInventoryDispositionRequest request);

		Task<IEnumerable<InventoryDispositionResponse>> GetAllAsync();

		Task<InventoryDispositionResponse> GetByIdAsync(Guid id);

		Task<InventoryDispositionResponse> UpdateAsync(Guid id, UpdateInventoryDispositionRequest request);

		Task DeleteAsync(Guid id);
	}
}
