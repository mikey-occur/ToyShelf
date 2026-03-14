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
		private readonly IUnitOfWork _unitOfWork;

		public InventoryService(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork)
		{
			_inventoryRepository = inventoryRepository;
			_unitOfWork = unitOfWork;
		}
		public async Task<InventoryResponse> RefillAsync(RefillInventoryRequest request)
		{
			if (request.Quantity <= 0)
				throw new AppException("Quantity must be greater than 0", 400);

			var inventory = await _inventoryRepository.GetAsync(
				request.InventoryLocationId,
				request.ProductColorId,
				InventoryStatus.Available);

			if (inventory == null)
			{
				inventory = new Inventory
				{
					Id = Guid.NewGuid(),
					InventoryLocationId = request.InventoryLocationId,
					ProductColorId = request.ProductColorId,
					Status = InventoryStatus.Available,
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
			InventoryStatus? status)
		{
			var inventories = await _inventoryRepository.GetAllInventoryAsync();

			var query = inventories.AsQueryable();

			if (locationId.HasValue)
			{
				query = query.Where(x => x.InventoryLocationId == locationId.Value);
			}

			if (status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value);
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
			foreach (var item in order.OrderItems)
			{
				var inventory = await _inventoryRepository
					.GetInventoryAsync(order.StoreId, item.ProductColorId, InventoryStatus.Available);

				if (inventory == null)
					throw new Exception($"Product {item.ProductColorId} not found in store.");

				if (inventory.Quantity < item.Quantity)
					throw new Exception($"Quantity not enough (available: {inventory.Quantity}, need: {item.Quantity}).");

				inventory.Quantity -= item.Quantity;

				_inventoryRepository.Update(inventory);
			}

			await _unitOfWork.SaveChangesAsync();
		}

		private static InventoryResponse MapToResponse(Inventory inventory)
		{
			return new InventoryResponse
			{
				Id = inventory.Id,
				InventoryLocationId = inventory.InventoryLocationId,
				ProductColorId = inventory.ProductColorId,
				Status = inventory.Status,
				Quantity = inventory.Quantity
			};
		}
	}
}
