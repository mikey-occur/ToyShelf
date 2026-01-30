using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Warehouse.Request;
using ToyShelf.Application.Models.Warehouse.Response;

namespace ToyShelf.Application.IServices
{
	public interface IWarehouseService
	{
		Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest request);

		Task<IEnumerable<WarehouseResponse>> GetWarehousesAsync(bool? isActive);
		Task<WarehouseResponse> GetByIdAsync(Guid id);

		Task<WarehouseResponse> UpdateAsync(Guid id, UpdateWarehouseRequest request);

		Task DisableAsync(Guid id);
		Task RestoreAsync(Guid id);

		Task DeleteAsync(Guid id);
	}
}
