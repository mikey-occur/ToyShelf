using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IUserWarehouseRepository : IGenericRepository<UserWarehouse>
	{
		Task<UserWarehouse?> GetActiveAsync(Guid userId, Guid warehouseId);
		Task<List<UserWarehouse>> GetUsersByWarehouseIdAsync(
		Guid warehouseId,
		WarehouseRole? role);
	}
}
