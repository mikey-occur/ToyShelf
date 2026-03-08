using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class InventoryService : IInventoryService
	{
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IUnitOfWork _unitOfWork;

		public InventoryService(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork)
		{
			_inventoryRepository = inventoryRepository;
			_unitOfWork = unitOfWork;
		}

		public async Task UpdateStockAfterPaymentAsync(Order order)
		{
			// Quy ước Code trạng thái hàng sẵn sàng bán
			const string availableCode = "AVAILABLE";

			foreach (var item in order.OrderItems)
			{
				// 1. Tìm bản ghi tồn kho tại Store cụ thể cho sản phẩm này
				var inventory = await _inventoryRepository.GetInventoryAsync(order.StoreId, item.ProductColorId, availableCode);

				// 2. Kiểm tra tính hợp lệ
				if (inventory == null)
				{
					throw new Exception($"Product {item.ProductColorId} Not found in store.");
				}

				if (inventory.Quantity < item.Quantity)
				{
					throw new Exception($"Quantity not enounh (avaiable: {inventory.Quantity}, Need: {item.Quantity}).");
				}

				// 3. Thực hiện trừ số lượng
				inventory.Quantity -= item.Quantity;

				
			}
		}
	}
}
