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
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IUnitOfWork _unitOfWork;

		public InventoryService(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork, IWarehouseRepository warehouseRepository)
		{
			_inventoryRepository = inventoryRepository;
			_unitOfWork = unitOfWork;
			_warehouseRepository = warehouseRepository;
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
					throw new AppException($"Product {item.ProductColorId} not found in store.", 404);

				if (inventory.Quantity < item.Quantity)
					throw new AppException($"Quantity not enough (available: {inventory.Quantity}, need: {item.Quantity}).", 404);

				inventory.Quantity -= item.Quantity;

				_inventoryRepository.Update(inventory);
			}

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<WarehouseInventoryResponse> GetWarehouseInventoryAsync(Guid warehouseId)
		{
			// 1. Lấy warehouse trước
			var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);

			if (warehouse == null)
				throw new AppException("Warehouse not found", 404);

			// 2. Lấy inventory
			var inventories = await _inventoryRepository.GetByWarehouseIdAsync(warehouseId);

			// 3. Nếu chưa có hàng → vẫn trả warehouse bình thường
			if (!inventories.Any())
			{
				return new WarehouseInventoryResponse
				{
					WarehouseId = warehouse.Id,
					WarehouseName = warehouse.Name,
					Products = new List<ProductInventoryItem>()
				};
			}

			// 4. Group như bình thường
			var groupedProducts = inventories
				.GroupBy(i => i.ProductColor.ProductId)
				.Select(productGroup => new ProductInventoryItem
				{
					ProductId = productGroup.Key,
					ProductName = productGroup.First().ProductColor.Product.Name,

					Colors = productGroup
						.GroupBy(i => i.ProductColorId)
						.Select(colorGroup => new ColorInventoryItem
						{
							ProductColorId = colorGroup.Key,
							ColorName = colorGroup.First().ProductColor.Color.Name,
							Quantity = colorGroup.Sum(x => x.Quantity)
						})
						.ToList()
				})
				.ToList();

			return new WarehouseInventoryResponse
			{
				WarehouseId = warehouse.Id,
				WarehouseName = warehouse.Name,
				Products = groupedProducts
			};
		}

		public async Task<LocationInventoryOverviewResponse> GetLocationInventoryOverviewAsync(Guid locationId)
		{
			// Lấy location (Warehouse hoặc Store)
			var location = await _inventoryRepository.GetLocationByIdAsync(locationId);
			if (location == null)
				throw new AppException("Location not found", 404);

			// Lấy tất cả inventory trong location này
			var inventories = await _inventoryRepository.GetByLocationAsync(locationId);

			if (!inventories.Any())
			{
				return new LocationInventoryOverviewResponse
				{
					LocationId = location.Id,
					LocationName = location.Name,
					Type = location.Type,
					Products = new List<ProductInventoryOverviewItem>()
				};
			}

			var groupedProducts = inventories
				.Where(i => i.ProductColor != null && i.ProductColor.Product != null)
				.GroupBy(i => i.ProductColor.ProductId)
				.Select(productGroup => new ProductInventoryOverviewItem
				{
					ProductId = productGroup.Key,
					ProductName = productGroup.First().ProductColor.Product.Name,
					Colors = productGroup
						.Where(i => i.ProductColor.Color != null)
						.GroupBy(i => i.ProductColorId)
						.Select(colorGroup => new ColorInventoryOverviewItem
						{
							ProductColorId = colorGroup.Key,
							ColorName = colorGroup.First().ProductColor.Color.Name,
							Available = colorGroup.Where(x => x.Status == InventoryStatus.Available).Sum(x => x.Quantity),
							InTransit = colorGroup.Where(x => x.Status == InventoryStatus.InTransit).Sum(x => x.Quantity),
							Damaged = colorGroup.Where(x => x.Status == InventoryStatus.Damaged).Sum(x => x.Quantity),
							Sold = colorGroup.Where(x => x.Status == InventoryStatus.Sold).Sum(x => x.Quantity)
						})
						.ToList()
				})
				.ToList();

			return new LocationInventoryOverviewResponse
			{
				LocationId = location.Id,
				LocationName = location.Name,
				Type = location.Type,
				Products = groupedProducts
			};
		}

		public async Task<IEnumerable<GlobalInventoryResponse>> GetGlobalInventoryAsync(InventoryLocationType? type)
		{
			var inventories = await _inventoryRepository.GetAllInventoryWithDetailsAsync(type);

			inventories = inventories.Where(i => i.InventoryLocation != null).ToList();

			var result = inventories
				.GroupBy(i => i.InventoryLocationId) // group theo InventoryLocationId
				.Select(locationGroup =>
				{
					var location = locationGroup.First().InventoryLocation!;
					return new GlobalInventoryResponse
					{
						InventoryLocationId = location.Id,  // ID trung gian
						Id = location.Type == InventoryLocationType.Warehouse
								? location.WarehouseId ?? Guid.Empty
								: location.StoreId ?? Guid.Empty, // ID thực Warehouse/Store
						Name = location.Name,
						Type = location.Type,
						Products = locationGroup
							.Where(i => i.ProductColor != null && i.ProductColor.Product != null)
							.GroupBy(i => i.ProductColor.ProductId)
							.Select(productGroup =>
							{
								var product = productGroup.First().ProductColor.Product!;
								return new GlobalProductInventoryItem
								{
									ProductId = product.Id,
									ProductName = product.Name,
									Colors = productGroup
										.Where(x => x.ProductColor.Color != null)
										.GroupBy(i => i.ProductColorId)
										.Select(colorGroup =>
										{
											var color = colorGroup.First().ProductColor.Color!;
											return new GlobalColorInventoryItem
											{
												ColorId = color.Id,
												ColorName = color.Name,
												Available = colorGroup
													.Where(x => x.Status == InventoryStatus.Available)
													.Sum(x => x.Quantity),
												InTransit = colorGroup
													.Where(x => x.Status == InventoryStatus.InTransit)
													.Sum(x => x.Quantity),
												Damaged = colorGroup
													.Where(x => x.Status == InventoryStatus.Damaged)
													.Sum(x => x.Quantity),
												Sold = colorGroup
													.Where(x => x.Status == InventoryStatus.Sold)
													.Sum(x => x.Quantity)
											};
										})
										.ToList()
								};
							})
							.ToList()
					};
				})
				.ToList();

			return result;
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
