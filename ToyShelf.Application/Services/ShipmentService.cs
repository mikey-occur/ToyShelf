using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.Shipment.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ShipmentService : IShipmentService
	{
		private readonly IShipmentRepository _shipmentRepository;
		private readonly IShipmentAssignmentRepository _assignmentRepository;
		private readonly IShipmentItemRepository _shipmentItemRepository;
		private readonly IShipmentMediaRepository _shipmentMediaRepository;
		private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IShelfShipmentItemRepository _shelfShipmentItemRepository;
		private readonly IShelfRepository _shelfRepository;
		private readonly IShelfTransactionRepository _shelfTransactionRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		private const string Prefix = "SH";

		public ShipmentService(
			IShipmentRepository shipmentRepository,
			IShipmentAssignmentRepository assignmentRepository,
			IShipmentItemRepository shipmentItemRepository,
			IShipmentMediaRepository shipmentMediaRepository,
			IInventoryTransactionRepository inventoryTransactionRepository,
			IInventoryRepository inventoryRepository,
			IShelfShipmentItemRepository shelfShipmentItemRepository,
			IShelfRepository shelfRepository,
			IShelfTransactionRepository shelfTransactionRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_shipmentRepository = shipmentRepository;
			_assignmentRepository = assignmentRepository;
			_shipmentItemRepository = shipmentItemRepository;
			_shipmentMediaRepository = shipmentMediaRepository;
			_inventoryTransactionRepository = inventoryTransactionRepository;
			_inventoryRepository = inventoryRepository;
			_shelfShipmentItemRepository = shelfShipmentItemRepository;
			_shelfRepository = shelfRepository;
			_shelfTransactionRepository = shelfTransactionRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		public async Task<IEnumerable<ShipmentResponse>> GetAllAsync(ShipmentStatus? shipmentStatus)
		{
			var shipments = await _shipmentRepository.GetAllWithDetailsAsync(shipmentStatus);

			return shipments.Select(MapToResponse);
		}
		public async Task<IEnumerable<ShipmentResponse>> GetByAssignmentIdAsync(Guid assignmentId)
		{
			var shipments = await _shipmentRepository.GetListByAssignmentIdAsync(assignmentId);

			if (shipments == null || !shipments.Any())
				throw new AppException("No shipments found", 404);

			return shipments.Select(MapToResponse);
		}

		public async Task<ShipmentResponse> GetByIdAsync(Guid shipmentId)
		{
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			return MapToResponse(shipment);
		}
		public async Task<IEnumerable<ShipmentResponse>> GetByStoreOrderIdAsync(Guid storeOrderId)
		{
			var shipments = await _shipmentRepository.GetByStoreOrderIdAsync(storeOrderId);

			if (shipments == null || !shipments.Any())
				throw new AppException("No shipments found for this store order", 404);

			return shipments.Select(MapToResponse);
		}

		public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, ICurrentUser currentUser)
		{
			if (request.Items == null || !request.Items.Any())
				throw new AppException("Shipment items are required", 400);

			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			if (assignment.Status == AssignmentStatus.Rejected)
				throw new AppException("Shipper has rejected this assignment", 400);

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment first", 400);

			if (assignment.ShipperId == null)
				throw new AppException("Shipper not assigned", 400);

			var isStoreOrder = assignment.StoreOrderId != null;

			var code = await GenerateCode();

			var shipment = new Shipment
			{
				Id = Guid.NewGuid(),
				Code = code,
				StoreOrderId = assignment.StoreOrderId,
				ShelfOrderId = assignment.ShelfOrderId,
				ShipmentAssignmentId = assignment.Id,
				FromLocationId = assignment.WarehouseLocationId,
				ToLocationId = isStoreOrder
					? assignment.StoreOrder!.StoreLocationId
					: assignment.ShelfOrder!.StoreLocationId,
				RequestedByUserId = currentUser.UserId,
				ShipperId = assignment.ShipperId,
				Status = ShipmentStatus.Draft,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentRepository.AddAsync(shipment);

			// ================= STORE ORDER =================
			if (isStoreOrder)
			{
				foreach (var reqItem in request.Items)
				{
					if (reqItem.ProductColorId == null)
						throw new AppException("ProductColorId is required", 400);

					var orderItem = assignment.StoreOrder!.Items
						.FirstOrDefault(x => x.ProductColorId == reqItem.ProductColorId);

					if (orderItem == null)
						throw new AppException("Invalid product", 400);

					var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;

					if (reqItem.ExpectedQuantity <= 0 || reqItem.ExpectedQuantity > remaining)
						throw new AppException("Invalid or exceeding quantity", 400);

					var inventory = await _inventoryRepository.GetByLocationAndProductAsync(
						assignment.WarehouseLocationId,
						orderItem.ProductColorId
					);

					if (inventory == null || inventory.Quantity < reqItem.ExpectedQuantity)
						throw new AppException($"Not enough inventory in warehouse '{assignment.WarehouseLocation.Name}'", 400);

					var shipmentItem = new ShipmentItem
					{
						Id = Guid.NewGuid(),
						ShipmentId = shipment.Id,
						ProductColorId = orderItem.ProductColorId,
						ExpectedQuantity = reqItem.ExpectedQuantity,
						ReceivedQuantity = 0
					};

					await _shipmentItemRepository.AddAsync(shipmentItem);
				}
			}
			// ================= SHELF ORDER =================
			else
			{
				foreach (var reqItem in request.Items)
				{
					if (reqItem.ShelfTypeId == null)
						throw new AppException("ShelfTypeId is required", 400);

					var orderItem = assignment.ShelfOrder!.Items
						.FirstOrDefault(x => x.ShelfTypeId == reqItem.ShelfTypeId);

					if (orderItem == null)
						throw new AppException("Invalid shelf item", 400);

					var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;

					if (reqItem.ExpectedQuantity <= 0 || reqItem.ExpectedQuantity > remaining)
						throw new AppException("Invalid or exceeding quantity", 400);

					var shelfItem = new ShelfShipmentItem
					{
						Id = Guid.NewGuid(),
						ShipmentId = shipment.Id,
						ShelfTypeId = orderItem.ShelfTypeId,
						ExpectedQuantity = reqItem.ExpectedQuantity,
						ReceivedQuantity = 0
					};

					await _shelfShipmentItemRepository.AddAsync(shelfItem);
				}
			}

			await _unitOfWork.SaveChangesAsync();

			var result = await _shipmentRepository.GetByIdWithDetailsAsync(shipment.Id);

			if (result == null)
				throw new AppException("Shipment not found", 404);

			return MapToResponse(result);
		}

		public async Task PickupAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			var shipment = await _shipmentRepository.GetByIdWithItemsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Draft)
				throw new AppException("Shipment not ready for pickup", 400);

			// Media 
			var media = new ShipmentMedia
			{
				Id = Guid.NewGuid(),
				ShipmentId = shipmentId,
				UploadedByUserId = currentUser.UserId,
				MediaUrl = request.MediaUrl,
				MediaType = ShipmentMediaType.Image,
				Purpose = ShipmentMediaPurpose.Pickup,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentMediaRepository.AddAsync(media);

			var isStoreOrder = shipment.StoreOrderId != null;

			// ================= STORE ORDER =================
			if (isStoreOrder)
			{
				foreach (var item in shipment.Items)
				{
					// INVENTORY OUT (Available -> InTransit)

					var inventoryAvailable = await _inventoryRepository
						.GetAsync(shipment.FromLocationId, item.ProductColorId, InventoryStatus.Available);

					if (inventoryAvailable == null || inventoryAvailable.Quantity < item.ExpectedQuantity)
						throw new AppException("Not enough stock in warehouse", 400);

					// trừ Available
					inventoryAvailable.Quantity -= item.ExpectedQuantity;

					// cộng InTransit
					var inventoryTransit = await _inventoryRepository
						.GetAsync(shipment.FromLocationId, item.ProductColorId, InventoryStatus.InTransit);

					if (inventoryTransit == null)
					{
						inventoryTransit = new Inventory
						{
							Id = Guid.NewGuid(),
							InventoryLocationId = shipment.FromLocationId,
							ProductColorId = item.ProductColorId,
							Status = InventoryStatus.InTransit,
							Quantity = item.ExpectedQuantity
						};

						await _inventoryRepository.AddAsync(inventoryTransit);
					}
					else
					{
						inventoryTransit.Quantity += item.ExpectedQuantity;
					}

					// Transaction
					var transaction = new InventoryTransaction
					{
						Id = Guid.NewGuid(),
						ProductColorId = item.ProductColorId,
						FromLocationId = shipment.FromLocationId,
						ToLocationId = shipment.ToLocationId,
						FromStatus = InventoryStatus.Available,
						ToStatus = InventoryStatus.InTransit,
						Quantity = item.ExpectedQuantity,
						ReferenceType = InventoryReferenceType.Shipment,
						ReferenceId = shipment.Id,
						CreatedAt = _dateTime.UtcNow
					};

					await _inventoryTransactionRepository.AddAsync(transaction);
				}
			}
			// ================= SHELF ORDER =================
			else
			{
				foreach (var item in shipment.ShelfShipmentItems)
				{
					// Lấy danh sách shelf Available theo type
					var shelves = await _shelfRepository
						.GetAvailableShelvesByType(shipment.FromLocationId, item.ShelfTypeId, item.ExpectedQuantity);

					//if (shelves.Count < item.ExpectedQuantity)
					//	throw new AppException("Not enough shelves in warehouse", 400);

					if (shelves == null || !shelves.Any())
						continue;

					foreach (var shelf in shelves)
					{
						// update status: Available -> InTransit
						shelf.Status = ShelfStatus.InTransit;

						// tạo transaction
						var transaction = new ShelfTransaction
						{
							Id = Guid.NewGuid(),
							ShelfId = shelf.Id,
							FromLocationId = shipment.FromLocationId,
							ToLocationId = shipment.ToLocationId,
							FromStatus = ShelfStatus.Available,
							ToStatus = ShelfStatus.InTransit,
							ReferenceType = ShelfReferenceType.Shipment,
							ReferenceId = shipment.Id,
							CreatedAt = _dateTime.UtcNow
						};

						await _shelfTransactionRepository.AddAsync(transaction);
					}
				}
			}

			shipment.Status = ShipmentStatus.Shipping;
			shipment.PickedUpAt = _dateTime.UtcNow;

			_shipmentRepository.Update(shipment);

			await _unitOfWork.SaveChangesAsync();
		}


		public async Task DeliveryAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Shipping)
				throw new AppException("Shipment not shipping", 400);

			var media = new ShipmentMedia
			{
				Id = Guid.NewGuid(),
				ShipmentId = shipmentId,
				UploadedByUserId = currentUser.UserId,
				MediaUrl = request.MediaUrl,
				MediaType = ShipmentMediaType.Image,
				Purpose = ShipmentMediaPurpose.Delivery,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentMediaRepository.AddAsync(media);

			shipment.Status = ShipmentStatus.Delivered;
			shipment.DeliveredAt = _dateTime.UtcNow;

			_shipmentRepository.Update(shipment);

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task ReceiveAsync(Guid shipmentId, ReceiveShipmentRequest request)
		{
			var shipment = await _shipmentRepository.GetByIdWithItemsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Delivered)
				throw new AppException("Shipment is not delivered yet", 400);

			var isStoreOrder = shipment.StoreOrderId != null;

			// ================= STORE ORDER =================
			if (isStoreOrder)
			{
				if (request.ProductItems == null || !request.ProductItems.Any())
					throw new AppException("Invalid product items", 400);

				foreach (var item in shipment.Items)
				{
					var reqItem = request.ProductItems
						.FirstOrDefault(x => x.ProductColorId == item.ProductColorId);

					if (reqItem == null)
						throw new AppException($"Missing item {item.ProductColorId}", 400);

					if (reqItem.ReceivedQuantity < 0 || reqItem.ReceivedQuantity > item.ExpectedQuantity)
						throw new AppException("Invalid quantity", 400);

					var receivedQty = reqItem.ReceivedQuantity;
					var damagedQty = item.ExpectedQuantity - receivedQty;

					item.ReceivedQuantity = receivedQty;

					var order = shipment.StoreOrder
						?? throw new AppException("Store order not found", 400);

					var orderItem = order.Items
						.FirstOrDefault(x => x.ProductColorId == item.ProductColorId);

					if (orderItem == null)
						throw new AppException("Order item not found", 400);

					if (orderItem.FulfilledQuantity + receivedQty > orderItem.Quantity)
						throw new AppException("Over fulfilled", 400);

					orderItem.FulfilledQuantity += receivedQty;

					// ===== INVENTORY =====

					var warehouseTransit = await _inventoryRepository
						.GetAsync(shipment.FromLocationId, item.ProductColorId, InventoryStatus.InTransit);

					if (warehouseTransit == null || warehouseTransit.Quantity < item.ExpectedQuantity)
						throw new AppException("Invalid transit stock", 400);

					warehouseTransit.Quantity -= item.ExpectedQuantity;

					// ===== Store nhận =====
					if (receivedQty > 0)
					{
						var storeAvailable = await _inventoryRepository
							.GetAsync(shipment.ToLocationId, item.ProductColorId, InventoryStatus.Available);

						if (storeAvailable == null)
						{
							storeAvailable = new Inventory
							{
								Id = Guid.NewGuid(),
								InventoryLocationId = shipment.ToLocationId,
								ProductColorId = item.ProductColorId,
								Status = InventoryStatus.Available,
								Quantity = 0
							};

							await _inventoryRepository.AddAsync(storeAvailable);
						}

						storeAvailable.Quantity += receivedQty;
					}

					// ===== Hàng hư =====
					if (damagedQty > 0)
					{
						var damagedInventory = await _inventoryRepository
							.GetAsync(shipment.FromLocationId, item.ProductColorId, InventoryStatus.Damaged);

						if (damagedInventory == null)
						{
							damagedInventory = new Inventory
							{
								Id = Guid.NewGuid(),
								InventoryLocationId = shipment.FromLocationId,
								ProductColorId = item.ProductColorId,
								Status = InventoryStatus.Damaged,
								Quantity = 0
							};

							await _inventoryRepository.AddAsync(damagedInventory);
						}

						damagedInventory.Quantity += damagedQty;
					}

					// ===== TRANSACTION =====
					if (receivedQty > 0)
					{
						await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
						{
							Id = Guid.NewGuid(),
							ProductColorId = item.ProductColorId,
							FromLocationId = shipment.FromLocationId,
							ToLocationId = shipment.ToLocationId,
							FromStatus = InventoryStatus.InTransit,
							ToStatus = InventoryStatus.Available,
							Quantity = receivedQty,
							ReferenceType = InventoryReferenceType.Shipment,
							ReferenceId = shipment.Id,
							CreatedAt = _dateTime.UtcNow
						});
					}

					if (damagedQty > 0)
					{
						await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
						{
							Id = Guid.NewGuid(),
							ProductColorId = item.ProductColorId,
							FromLocationId = shipment.FromLocationId,
							ToLocationId = shipment.FromLocationId,
							FromStatus = InventoryStatus.InTransit,
							ToStatus = InventoryStatus.Damaged,
							Quantity = damagedQty,
							ReferenceType = InventoryReferenceType.Shipment,
							ReferenceId = shipment.Id,
							CreatedAt = _dateTime.UtcNow
						});
					}
				}

				// ===== UPDATE ORDER STATUS =====
				var orderFinal = shipment.StoreOrder!;

				var totalOrdered = orderFinal.Items.Sum(x => x.Quantity);
				var totalFulfilled = orderFinal.Items.Sum(x => x.FulfilledQuantity);

				if (totalFulfilled == 0)
					orderFinal.Status = StoreOrderStatus.Approved;
				else if (totalFulfilled < totalOrdered)
					orderFinal.Status = StoreOrderStatus.PartiallyFulfilled;
				else
					orderFinal.Status = StoreOrderStatus.Fulfilled;
			}

			// ================= SHELF ORDER =================
			else
			{
				if (request.ShelfItems == null || !request.ShelfItems.Any())
					throw new AppException("Invalid shelf items", 400);

				var shelfOrder = shipment.ShelfOrder
					?? throw new AppException("Shelf order not found", 400);

				foreach (var item in shipment.ShelfShipmentItems)
				{
					// lấy tất cả shelf thuộc shipment + type
					var shelves = await _shelfTransactionRepository
						.GetShelvesByShipmentAndType(shipment.Id, item.ShelfTypeId);

					if (shelves == null || !shelves.Any())
						continue;

					int receivedCount = 0;

					foreach (var shelf in shelves)
					{
						var reqShelf = request.ShelfItems
							.FirstOrDefault(x => x.ShelfId == shelf.Id);

						if (reqShelf == null)
							throw new AppException($"Missing shelf {shelf.Id}", 400);

						if (reqShelf.IsReceived)
						{
							shelf.InventoryLocationId = shipment.ToLocationId;
							shelf.Status = ShelfStatus.InUse;
							shelf.AssignedAt = _dateTime.UtcNow;

							receivedCount++;

							await _shelfTransactionRepository.AddAsync(new ShelfTransaction
							{
								Id = Guid.NewGuid(),
								ShelfId = shelf.Id,
								FromLocationId = shipment.FromLocationId,
								ToLocationId = shipment.ToLocationId,
								FromStatus = ShelfStatus.InTransit,
								ToStatus = ShelfStatus.InUse,
								ReferenceType = ShelfReferenceType.Shipment,
								ReferenceId = shipment.Id,
								CreatedAt = _dateTime.UtcNow
							});
						}
						else
						{
							shelf.Status = ShelfStatus.Maintenance;

							await _shelfTransactionRepository.AddAsync(new ShelfTransaction
							{
								Id = Guid.NewGuid(),
								ShelfId = shelf.Id,
								FromLocationId = shipment.FromLocationId,
								ToLocationId = shipment.FromLocationId,
								FromStatus = ShelfStatus.InTransit,
								ToStatus = ShelfStatus.Maintenance,
								ReferenceType = ShelfReferenceType.Shipment,
								ReferenceId = shipment.Id,
								CreatedAt = _dateTime.UtcNow
							});
						}
					}

					item.ReceivedQuantity = receivedCount;

					var orderItem = shelfOrder.Items
						.FirstOrDefault(x => x.ShelfTypeId == item.ShelfTypeId);

					if (orderItem == null)
						throw new AppException("Shelf order item not found", 400);

					if (orderItem.FulfilledQuantity + receivedCount > orderItem.Quantity)
						throw new AppException("Over fulfilled", 400);

					orderItem.FulfilledQuantity += receivedCount;
				}

				var totalOrdered = shelfOrder.Items.Sum(x => x.Quantity);
				var totalFulfilled = shelfOrder.Items.Sum(x => x.FulfilledQuantity);

				if (totalFulfilled == 0)
					shelfOrder.Status = ShelfOrderStatus.Approved;
				else if (totalFulfilled < totalOrdered)
					shelfOrder.Status = ShelfOrderStatus.PartiallyFulfilled;
				else
					shelfOrder.Status = ShelfOrderStatus.Fulfilled;
			}

			shipment.Status = ShipmentStatus.Received;
			shipment.ReceivedAt = _dateTime.UtcNow;

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<IEnumerable<ShelfSimpleResponse>> GetShelvesByShipmentAsync(Guid shipmentId)
		{
			var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			var shelves = await _shelfTransactionRepository.GetShelvesByShipment(shipmentId);

			return shelves.Select(s => new ShelfSimpleResponse
			{
				Id = s.Id,
				Code = s.Code,
				Status = s.Status
			});
		}

		private async Task<string> GenerateCode()
		{
			var max = await _shipmentRepository.GetMaxSequenceAsync();

			return $"{Prefix}-{(max + 1):D5}";
		}

		private static ShipmentResponse MapToResponse(Shipment shipment)
		{
			var isStoreOrder = shipment.StoreOrderId != null;

			var response = new ShipmentResponse
			{
				Id = shipment.Id,
				Code = shipment.Code,

				StoreOrderId = shipment.StoreOrderId,
				ShelfOrderId = shipment.ShelfOrderId,

				OrderType = isStoreOrder ? "STORE" : "SHELF",

				FromLocationId = shipment.FromLocationId,
				FromLocationName = shipment.FromLocation.Name,

				ToLocationId = shipment.ToLocationId,
				ToLocationName = shipment.ToLocation.Name,

				ShipperName = shipment.ShipmentAssignment?.Shipper?.FullName,

				Status = shipment.Status,
				CreatedAt = shipment.CreatedAt,
				PickedUpAt = shipment.PickedUpAt,
				DeliveredAt = shipment.DeliveredAt,
				ReceivedAt = shipment.ReceivedAt
			};

			// ================= STORE ORDER =================
			if (isStoreOrder)
			{
				response.ProductItems = shipment.Items.Select(x => new ShipmentProductItemResponse
				{
					ProductColorId = x.ProductColorId,
					SKU = x.ProductColor.Product.SKU,
					ProductName = x.ProductColor.Product.Name,
					Color = x.ProductColor.Color.Name,
					ImageUrl = x.ProductColor.ImageUrl,
					ExpectedQuantity = x.ExpectedQuantity,
					ReceivedQuantity = x.ReceivedQuantity
				}).ToList();
			}
			// ================= SHELF ORDER =================
			else
			{
				response.ShelfItems = shipment.ShelfShipmentItems.Select(x => new ShipmentShelfItemResponse
				{
					ShelfTypeId = x.ShelfTypeId,
					ShelfTypeName = x.ShelfType.Name,
					ImageUrl = x.ShelfType.ImageUrl,
					ExpectedQuantity = x.ExpectedQuantity,
					ReceivedQuantity = x.ReceivedQuantity
				}).ToList();
			}

			return response;
		}
	}
}
