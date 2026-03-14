using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.InventoryLocation.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IInventoryLocationService
	{
		Task<IEnumerable<InventoryLocationResponse>> GetInventoryLocationsAsync(bool? isActive);
		Task<InventoryLocationResponse> GetByIdAsync(Guid id);
		Task DisableAsync(Guid id);
		Task RestoreAsync(Guid id);
	}
}
