using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Domain.IRepositories
{
	public interface IWarehouseRepository : IGenericRepository<Warehouse>
	{
		Task<IEnumerable<Warehouse>> GetWarehousesAsync(bool? isActive);
	}
}
