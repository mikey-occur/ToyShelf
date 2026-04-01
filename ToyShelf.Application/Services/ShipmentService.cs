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
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_shipmentRepository = shipmentRepository;
			_assignmentRepository = assignmentRepository;
			_shipmentItemRepository = shipmentItemRepository;
			_shipmentMediaRepository = shipmentMediaRepository;
			_inventoryTransactionRepository = inventoryTransactionRepository;
			_inventoryRepository = inventoryRepository;
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
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			if (assignment.Status == AssignmentStatus.Rejected)
				throw new AppException("Shipper has rejected this assignment", 400);

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment first", 400);

			var code = await GenerateCode();

			var shipment = new Shipment
			{
				Id = Guid.NewGuid(),
				Code = code,
				StoreOrderId = assignment.StoreOrderId,
				ShipmentAssignmentId = assignment.Id,
				FromLocationId = assignment.WarehouseLocationId,
				ToLocationId = assignment.StoreOrder.StoreLocationId,
				RequestedByUserId = currentUser.UserId,
				ShipperId = assignment.ShipperId,
				Status = ShipmentStatus.Draft,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentRepository.AddAsync(shipment);

			foreach (var reqItem in request.Items)
			{
				var orderItem = assignment.StoreOrder.Items
					.FirstOrDefault(x => x.ProductColorId == reqItem.ProductColorId);

				if (orderItem == null)
					throw new AppException("Invalid product", 400);

				var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;

				if (reqItem.ExpectedQuantity <= 0 || reqItem.ExpectedQuantity > remaining)
					throw new AppException("Invalid or exceeding quantity", 400);

				// Check tồn kho
				var inventory = await _inventoryRepository.GetByLocationAndProductAsync(
						assignment.WarehouseLocationId,
						orderItem.ProductColorId
					);

				if (inventory == null || inventory.Quantity < reqItem.ExpectedQuantity)
					throw new AppException($"Not enough inventory in warehouse '{assignment.WarehouseLocation.Name}' for product '{orderItem.ProductColor.Product.Name}'", 400);

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

			if (request.Items == null || !request.Items.Any())
				throw new AppException("Invalid request items", 400);

			foreach (var item in shipment.Items)
			{
				var reqItem = request.Items
					.FirstOrDefault(x => x.ProductColorId == item.ProductColorId);

				if (reqItem == null)
					throw new AppException($"Missing item {item.ProductColorId}", 400);

				if (reqItem.ReceivedQuantity < 0 || reqItem.ReceivedQuantity > item.ExpectedQuantity)
					throw new AppException("Invalid quantity", 400);

				var receivedQty = reqItem.ReceivedQuantity;
				var damagedQty = item.ExpectedQuantity - receivedQty;

				// Update Shipment Item
				item.ReceivedQuantity = receivedQty;

				// Update Order
				var orderItem = shipment.StoreOrder.Items
					.FirstOrDefault(x => x.ProductColorId == item.ProductColorId);

				if (orderItem == null)
					throw new AppException("Order item not found", 400);

				if (orderItem.FulfilledQuantity + receivedQty > orderItem.Quantity)
					throw new AppException("Over fulfilled", 400);

				orderItem.FulfilledQuantity += receivedQty;

				// Inventory

				// 1. Trừ FULL InTransit 
				var warehouseTransit = await _inventoryRepository
					.GetAsync(shipment.FromLocationId, item.ProductColorId, InventoryStatus.InTransit);

				if (warehouseTransit == null || warehouseTransit.Quantity < item.ExpectedQuantity)
					throw new AppException("Invalid transit stock", 400);

				warehouseTransit.Quantity -= item.ExpectedQuantity;

				// 2. Store nhận hàng 
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

				// 3. Hàng hư 
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

				// Transaction

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

			// Update order status
			var order = shipment.StoreOrder;

			var totalOrdered = order.Items.Sum(x => x.Quantity);
			var totalFulfilled = order.Items.Sum(x => x.FulfilledQuantity);

			if (totalFulfilled == 0)
			{
				order.Status = StoreOrderStatus.Approved;
			}
			else if (totalFulfilled < totalOrdered)
			{
				order.Status = StoreOrderStatus.PartiallyFulfilled; 
			}
			else
			{
				order.Status = StoreOrderStatus.Fulfilled;
			}

			// Update shipment status
			shipment.Status = ShipmentStatus.Received;
			shipment.ReceivedAt = _dateTime.UtcNow;

			await _unitOfWork.SaveChangesAsync();
		}


		private async Task<string> GenerateCode()
		{
			var max = await _shipmentRepository.GetMaxSequenceAsync();

			return $"{Prefix}-{(max + 1):D5}";
		}

		private static ShipmentResponse MapToResponse(Shipment shipment)
		{
			return new ShipmentResponse
			{
				Id = shipment.Id,
				Code = shipment.Code,
				StoreOrderId = shipment.StoreOrderId ?? Guid.Empty,
				FromLocationId = shipment.FromLocationId,
				FromLocationName = shipment.FromLocation.Name,
				ToLocationId = shipment.ToLocationId,
				ToLocationName = shipment.ToLocation.Name,
				ShipperName = shipment.ShipmentAssignment.Shipper?.FullName,
				Status = shipment.Status,
				CreatedAt = shipment.CreatedAt,
				PickedUpAt = shipment.PickedUpAt,
				DeliveredAt = shipment.DeliveredAt,
				ReceivedAt = shipment.ReceivedAt,
				Items = shipment.Items.Select(x => new ShipmentItemResponse
				{
					ProductColorId = x.ProductColorId,
					SKU = x.ProductColor.Product.SKU,
					ProductName = x.ProductColor.Product.Name,
					Color = x.ProductColor.Color.Name,
					ImageUrl = x.ProductColor.ImageUrl,
					ExpectedQuantity = x.ExpectedQuantity,
					ReceivedQuantity = x.ReceivedQuantity
				}).ToList()
			};
		}
	}
}
