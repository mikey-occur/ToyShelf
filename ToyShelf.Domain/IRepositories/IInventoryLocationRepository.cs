using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IInventoryLocationRepository : IGenericRepository<InventoryLocation>
	{
		Task<InventoryLocation?> GetByWarehouseIdAsync(Guid warehouseId);
		Task<InventoryLocation?> GetByStoreIdAsync(Guid storeId);
	}
}
