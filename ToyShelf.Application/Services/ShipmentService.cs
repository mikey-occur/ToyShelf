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
			// 1. Validation đầu vào cơ bản
			if (request.Items == null || !request.Items.Any())
				throw new AppException("Shipment items are required", 400);

			// 2. Lấy Assignment kèm chi tiết các bảng trung gian (N-N)
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 3. Kiểm tra trạng thái Shipper (Chỉ cho phép tạo khi đã Accepted)
			if (assignment.Status == AssignmentStatus.Rejected)
				throw new AppException("Shipper has rejected this assignment", 400);

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment first", 400);

			if (assignment.ShipperId == null)
				throw new AppException("Shipper not assigned", 400);

			// 4. Khởi tạo Shipment
			var code = await GenerateCode();

			// Ép kiểu về Guid? để FirstOrDefault trả về null nếu danh sách trống
			var toLocationId = assignment.AssignmentStoreOrders
								   .Select(x => (Guid?)x.StoreOrder.StoreLocationId)
								   .FirstOrDefault()
							   ?? assignment.AssignmentShelfOrders
								   .Select(x => (Guid?)x.ShelfOrder.StoreLocationId)
								   .FirstOrDefault()
							   ?? assignment.DamageReports
								   .Select(x => (Guid?)x.InventoryLocationId)
								   .FirstOrDefault();

			// Kiểm tra nếu cuối cùng vẫn không tìm thấy địa điểm nào
			if (toLocationId == null)
				throw new AppException("Could not determine destination location for this shipment", 400);

			var shipment = new Shipment
			{
				Id = Guid.NewGuid(),
				Code = code,
				ShipmentAssignmentId = assignment.Id,
				FromLocationId = assignment.WarehouseLocationId,
				ToLocationId = toLocationId.Value,
				RequestedByUserId = currentUser.UserId,
				ShipperId = assignment.ShipperId,
				Status = ShipmentStatus.Draft,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentRepository.AddAsync(shipment);

			// 5. XỬ LÝ DANH SÁCH ITEMS TRONG REQUEST
			foreach (var reqItem in request.Items)
			{
				// ================= TRƯỜNG HỢP: GIAO SẢN PHẨM (STORE PRODUCT) =================
				if (reqItem.ProductColorId != null)
				{
					var expectedQty = reqItem.ExpectedQuantity ?? throw new AppException("ExpectedQuantity is required", 400);

					// Tìm Item này trong toàn bộ các Store Orders được gán cho xe
					var orderItem = assignment.AssignmentStoreOrders
						.SelectMany(x => x.StoreOrder.Items)
						.FirstOrDefault(x => x.ProductColorId == reqItem.ProductColorId);

					if (orderItem == null)
						throw new AppException($"Product {reqItem.ProductColorId} not found in any assigned orders", 400);

					var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;
					if (expectedQty <= 0 || expectedQty > remaining)
						throw new AppException($"Invalid quantity for product {reqItem.ProductColorId}. Max remaining: {remaining}", 400);

					// Check tồn kho Available tại Warehouse nguồn
					var inventory = await _inventoryRepository.GetByLocationAndProductAsync(assignment.WarehouseLocationId, orderItem.ProductColorId);
					if (inventory == null || inventory.Quantity < expectedQty)
						throw new AppException($"Not enough stock for product {reqItem.ProductColorId} in warehouse", 400);

					await _shipmentItemRepository.AddAsync(new ShipmentItem
					{
						Id = Guid.NewGuid(),
						ShipmentId = shipment.Id,
						ProductColorId = orderItem.ProductColorId,
						ExpectedQuantity = expectedQty,
						ReceivedQuantity = 0
					});

					// Chuyển trạng thái StoreOrder sang Processing
					if (orderItem.StoreOrder.Status == StoreOrderStatus.Approved)
						orderItem.StoreOrder.Status = StoreOrderStatus.Processing;
				}

				// ================= TRƯỜNG HỢP: GIAO KỆ (SHELF) =================
				else if (reqItem.ShelfTypeId != null || (reqItem.ShelfIds != null && reqItem.ShelfIds.Any()))
				{
					var isManual = reqItem.ShelfIds != null && reqItem.ShelfIds.Any();
					Guid shelfTypeId;
					List<Shelf> shelvesToReserve;

					if (isManual)
					{
						// LUỒNG MANUAL: Admin chọn đích danh ID kệ
						if (reqItem.ShelfTypeId != null || reqItem.ExpectedQuantity != null)
							throw new AppException("Manual mode cannot use ShelfTypeId or ExpectedQuantity", 400);

						shelvesToReserve = await _shelfRepository.GetByIds(reqItem.ShelfIds!);
						if (shelvesToReserve.Count != reqItem.ShelfIds!.Count)
							throw new AppException("Some shelves not found", 400);

						shelfTypeId = shelvesToReserve.First().ShelfTypeId;
						if (shelvesToReserve.Any(x => x.ShelfTypeId != shelfTypeId))
							throw new AppException("All selected shelves must be the same type", 400);
					}
					else
					{
						// LUỒNG AUTO: Hệ thống tự bốc kệ theo số lượng
						if (reqItem.ShelfTypeId == null || reqItem.ExpectedQuantity == null)
							throw new AppException("ShelfTypeId and ExpectedQuantity are required for Auto mode", 400);

						shelfTypeId = reqItem.ShelfTypeId.Value;
						var quantity = reqItem.ExpectedQuantity.Value;

						shelvesToReserve = await _shelfRepository.GetAvailableShelvesByType(
							assignment.WarehouseLocationId,
							shelfTypeId,
							quantity
						);

						if (shelvesToReserve.Count < quantity)
							throw new AppException($"Not enough available shelves of type {shelfTypeId} in warehouse", 400);
					}

					// Tìm Shelf Order Item tương ứng trong Assignment
					var shelfOrderItem = assignment.AssignmentShelfOrders
						.SelectMany(x => x.ShelfOrder.Items)
						.FirstOrDefault(x => x.ShelfTypeId == shelfTypeId);

					if (shelfOrderItem == null)
						throw new AppException("This shelf type is not in the assigned orders", 400);

					var remainingShelf = shelfOrderItem.Quantity - shelfOrderItem.FulfilledQuantity;
					if (shelvesToReserve.Count > remainingShelf)
						throw new AppException($"Exceeding remaining quantity for shelf type. Max allowed: {remainingShelf}", 400);

					// Cập nhật trạng thái từng cái kệ và lưu Shipment Item
					foreach (var shelf in shelvesToReserve)
					{
						if (shelf.Status != ShelfStatus.Available || shelf.InventoryLocationId != assignment.WarehouseLocationId)
							throw new AppException($"Shelf {shelf.Code} is not available or in wrong location", 400);

						shelf.Status = ShelfStatus.Reserved; // Đánh dấu giữ chỗ

						await _shelfShipmentItemRepository.AddAsync(new ShelfShipmentItem
						{
							Id = Guid.NewGuid(),
							ShipmentId = shipment.Id,
							ShelfId = shelf.Id,
							Status = ShelfShipmentStatus.InTransit
						});
					}

					// Chuyển trạng thái ShelfOrder sang Processing
					if (shelfOrderItem.ShelfOrder.Status == ShelfOrderStatus.Approved)
						shelfOrderItem.ShelfOrder.Status = ShelfOrderStatus.Processing;
				}
			}

			// 6. Lưu toàn bộ thay đổi vào database
			await _unitOfWork.SaveChangesAsync();

			// 7. Map kết quả trả về
			var result = await _shipmentRepository.GetByIdWithDetailsAsync(shipment.Id);
			return MapToResponse(result!);
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
					var shelf = await _shelfRepository.GetByIdAsync(item.ShelfId);

					if (shelf == null)
						throw new AppException($"Shelf {item.ShelfId} not found", 404);

					// chỉ cho pickup khi đã Reserved
					if (shelf.Status != ShelfStatus.Reserved)
						throw new AppException($"Shelf {shelf.Id} is not ready for pickup", 400);

					if (shelf.InventoryLocationId != shipment.FromLocationId)
						throw new AppException($"Shelf {shelf.Id} is not in this warehouse", 400);

					// chuyển trạng thái
					shelf.Status = ShelfStatus.InTransit;

					// transaction
					var transaction = new ShelfTransaction
					{
						Id = Guid.NewGuid(),
						ShelfId = shelf.Id,
						FromLocationId = shipment.FromLocationId,
						ToLocationId = shipment.ToLocationId,
						FromStatus = ShelfStatus.Reserved,
						ToStatus = ShelfStatus.InTransit,
						ReferenceType = ShelfReferenceType.Shipment,
						ReferenceId = shipment.Id,
						CreatedAt = _dateTime.UtcNow
					};

					await _shelfTransactionRepository.AddAsync(transaction);
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
					throw new AppException("ProductItems product items", 400);

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
					throw new AppException("ShelfItems is required", 400);

				var shelfOrder = shipment.ShelfOrder
					?? throw new AppException("Shelf order not found", 400);

				foreach (var item in shipment.ShelfShipmentItems)
				{
					var shelf = await _shelfRepository.GetByIdAsync(item.ShelfId);

					if (shelf == null)
						throw new AppException($"Shelf {item.ShelfId} not found", 404);

					var reqShelf = request.ShelfItems
						.FirstOrDefault(x => x.ShelfId == shelf.Id);

					if (reqShelf == null)
						throw new AppException($"Missing shelf {shelf.Id}", 400);

					// phải đang InTransit mới được receive
					if (shelf.Status != ShelfStatus.InTransit)
						throw new AppException($"Shelf {shelf.Id} is not in transit", 400);

					// ===== RECEIVED =====
					if (reqShelf.IsReceived)
					{
						shelf.InventoryLocationId = shipment.ToLocationId;
						shelf.Status = ShelfStatus.InUse;
						shelf.AssignedAt = _dateTime.UtcNow;

						item.Status = ShelfShipmentStatus.Received;

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

						// update order theo từng cái shelf
						var orderItem = shelfOrder.Items
							.FirstOrDefault(x => x.ShelfTypeId == shelf.ShelfTypeId);

						if (orderItem == null)
							throw new AppException("Shelf order item not found", 400);

						if (orderItem.FulfilledQuantity + 1 > orderItem.Quantity)
							throw new AppException("Over fulfilled", 400);

						orderItem.FulfilledQuantity += 1;
					}
					// ===== DAMAGED / FAIL =====
					else
					{
						shelf.Status = ShelfStatus.Maintenance;

						item.Status = ShelfShipmentStatus.Damaged;

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

				// ===== UPDATE ORDER STATUS =====
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

		public async Task<List<ShelfShipmentItemResponse>> GetShelfItemsAsync(Guid shipmentId)
		{
			var shipment = await _shipmentRepository.GetByIdWithShelfItemsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.StoreOrderId != null)
				throw new AppException("This shipment is not a shelf shipment", 400);

			var result = shipment.ShelfShipmentItems
				.Select(x => new ShelfShipmentItemResponse
				{
					ShelfId = x.ShelfId,
					Code = x.Shelf.Code,
					ShelfTypeName = x.Shelf.ShelfType.Name,
					Status = x.Status
				})
				.ToList();

			return result;
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
				response.ShelfItems = shipment.ShelfShipmentItems
					.Where(x => x.Shelf != null) 
					.GroupBy(x => x.Shelf.ShelfTypeId)
					.Select(g =>
					{
						var firstShelf = g.First().Shelf;
						var shelfType = firstShelf?.ShelfType;

						return new ShipmentShelfItemResponse
						{
							ShelfTypeId = g.Key,
							ShelfTypeName = shelfType?.Name ?? "Unknown",
							ImageUrl = shelfType?.ImageUrl ?? string.Empty,

							Width = shelfType?.Width ?? 0,
							Height = shelfType?.Height ?? 0,
							Depth = shelfType?.Depth ?? 0,
							TotalLevels = shelfType?.TotalLevels ?? 0,

							ExpectedQuantity = g.Count(),
							ReceivedQuantity = g.Count(x => x.Status == ShelfShipmentStatus.Received)
						};
					})
					.ToList();
			}
			return response;
		}
	}
}
