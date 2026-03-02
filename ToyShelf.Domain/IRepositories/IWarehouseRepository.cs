using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IWarehouseRepository : IGenericRepository<Warehouse>
	{
		Task<IEnumerable<Warehouse>> GetWarehousesAsync(bool? isActive);
		Task<Warehouse?> GetByIdWithCityAsync(Guid id);
		Task<int> CountByCityAsync(Guid cityId);
		Task<bool> ExistsByCodeInCityAsync(string code, Guid cityId);
		Task<int> GetMaxSequenceByCityAsync(Guid cityId);
	}
}
