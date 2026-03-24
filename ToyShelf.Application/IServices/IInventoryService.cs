using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Inventory.Request;
using ToyShelf.Application.Models.Inventory.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IInventoryService
	{
		Task<InventoryResponse> RefillAsync(RefillInventoryRequest request);
		Task<IEnumerable<InventoryResponse>> GetInventoriesAsync(
			Guid? locationId,
			InventoryStatus? status);
		Task<InventoryResponse> GetByIdAsync(Guid id);
		Task UpdateStockAfterPaymentAsync(Order order);
		Task<WarehouseInventoryResponse> GetWarehouseInventoryAsync(Guid warehouseId);
	}
}
