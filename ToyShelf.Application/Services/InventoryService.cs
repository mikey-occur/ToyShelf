using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Inventory.Request;
using ToyShelf.Application.Models.Inventory.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class InventoryService : IInventoryService
	{
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IInventoryDispositionRepository _dispositionRepository;
		private readonly IUnitOfWork _unitOfWork;

		public InventoryService(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork, IInventoryDispositionRepository dispositionRepository)
		{
			_inventoryRepository = inventoryRepository;
			_unitOfWork = unitOfWork;
			_dispositionRepository = dispositionRepository;
		}
		public async Task<InventoryResponse> RefillAsync(RefillInventoryRequest request)
		{
			if (request.Quantity <= 0)
				throw new AppException("Quantity must be greater than 0", 400);

			var availableDisposition = await _dispositionRepository
				.GetByCodeAsync("AVAILABLE");

			if (availableDisposition == null)
				throw new AppException("AVAILABLE disposition not found", 500);

			var inventory = await _inventoryRepository.GetAsync(
				request.InventoryLocationId,
				request.ProductColorId,
				availableDisposition.Id);

			if (inventory == null)
			{
				inventory = new Inventory
				{
					Id = Guid.NewGuid(),
					InventoryLocationId = request.InventoryLocationId,
					ProductColorId = request.ProductColorId,
					DispositionId = availableDisposition.Id,
					Quantity = request.Quantity
				};

				await _inventoryRepository.AddAsync(inventory);
			}
			else
			{
				inventory.Quantity += request.Quantity;
				_inventoryRepository.Update(inventory);
			}

			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(inventory);
		}

		public async Task<IEnumerable<InventoryResponse>> GetInventoriesAsync(
			Guid? locationId,
			Guid? dispositionId)
		{
			var inventories = await _inventoryRepository.GetAllAsync();

			var query = inventories.AsQueryable();

			if (locationId.HasValue)
			{
				query = query.Where(x => x.InventoryLocationId == locationId.Value);
			}

			if (dispositionId.HasValue)
			{
				query = query.Where(x => x.DispositionId == dispositionId.Value);
			}

			return query.Select(MapToResponse);
		}

		public async Task<InventoryResponse> GetByIdAsync(Guid id)
		{
			var inventory = await _inventoryRepository.GetByIdAsync(id);

			if (inventory == null)
				throw new AppException("Inventory not found", 404);

			return MapToResponse(inventory);
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
		private static InventoryResponse MapToResponse(Inventory inventory)
		{
			return new InventoryResponse
			{
				Id = inventory.Id,
				InventoryLocationId = inventory.InventoryLocationId,
				ProductColorId = inventory.ProductColorId,
				DispositionId = inventory.DispositionId,
				Quantity = inventory.Quantity
			};
		}

	}
}
