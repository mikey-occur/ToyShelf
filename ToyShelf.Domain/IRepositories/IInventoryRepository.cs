using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IInventoryRepository : IGenericRepository<Inventory>
	{
		Task<Inventory?> GetInventoryAsync(Guid storeId, Guid productColorId, string dispositionCode);
		Task<Inventory?> GetAsync(Guid locationId, Guid productColorId, Guid dispositionId);
		Task<IEnumerable<Inventory>> GetAllInventoryAsync();
		Task<IEnumerable<Inventory>> GetByLocationAsync(Guid locationId);
	}
}
