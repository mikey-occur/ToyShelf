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
		private readonly IDamageReportRepository _damageReportRepository;
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
			IDamageReportRepository damageReportRepository,
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
			_damageReportRepository = damageReportRepository;
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
			// 1. Validation đầu vào căn bản
			if (request.Items == null || !request.Items.Any())
				throw new AppException("Shipment items are required", 400);

			// 2. Lấy Assignment kèm chi tiết các quan hệ (N-N)
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 3. Kiểm tra trạng thái Shipper
			if (assignment.Status == AssignmentStatus.Rejected)
				throw new AppException("Shipper has rejected this assignment", 400);

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment first", 400);

			if (assignment.ShipperId == null)
				throw new AppException("Shipper not assigned to this assignment", 400);

			// 4. Xác định điểm đến (ToLocationId) cho Shipment này
			// Một Assignment có thể đi qua nhiều điểm, nhưng 1 Shipment chỉ có 1 đích đến
			var toLocationId = assignment.AssignmentStoreOrders
								.Select(x => (Guid?)x.StoreOrder.StoreLocationId)
								.FirstOrDefault()
							?? assignment.AssignmentShelfOrders
								.Select(x => (Guid?)x.ShelfOrder.StoreLocationId)
								.FirstOrDefault()
							?? assignment.AssignmentDamageReports
								.Select(x => (Guid?)x.DamageReport.InventoryLocationId)
								.FirstOrDefault();

			if (toLocationId == null)
				throw new AppException("Could not determine destination location for this shipment", 400);

			var shipment = new Shipment
			{
				Id = Guid.NewGuid(),
				Code = await GenerateCode(),
				ShipmentAssignmentId = assignment.Id,
				FromLocationId = assignment.WarehouseLocationId,
				ToLocationId = toLocationId.Value,
				RequestedByUserId = currentUser.UserId,
				ShipperId = assignment.ShipperId,
				Status = ShipmentStatus.Draft,
				CreatedAt = _dateTime.UtcNow
			};

			// 5. Liên kết các Orders/Reports thuộc ToLocation này vào Shipment
			var relatedStoreOrders = assignment.AssignmentStoreOrders
				.Where(x => x.StoreOrder.StoreLocationId == toLocationId)
				.Select(x => x.StoreOrder).ToList();

			var relatedShelfOrders = assignment.AssignmentShelfOrders
				.Where(x => x.ShelfOrder.StoreLocationId == toLocationId)
				.Select(x => x.ShelfOrder).ToList();

			relatedStoreOrders.ForEach(o => shipment.StoreOrders.Add(o));
			relatedShelfOrders.ForEach(o => shipment.ShelfOrders.Add(o));

			try
			{
				await _shipmentRepository.AddAsync(shipment);

				// 6. XỬ LÝ CHI TIẾT DANH SÁCH ITEMS
				foreach (var reqItem in request.Items)
				{
					// --- TRƯỜNG HỢP: GIAO SẢN PHẨM (STORE PRODUCT) ---
					if (reqItem.ProductColorId != null)
					{
						var expectedQty = reqItem.ExpectedQuantity ?? throw new AppException("ExpectedQuantity is required for products", 400);

						// Tìm trong các đơn hàng đã gom
						var orderItem = relatedStoreOrders.SelectMany(x => x.Items)
							.FirstOrDefault(x => x.ProductColorId == reqItem.ProductColorId);

						if (orderItem == null)
							throw new AppException($"Product {reqItem.ProductColorId} not found in assigned orders for this destination", 400);

						var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;
						if (expectedQty <= 0 || expectedQty > remaining)
							throw new AppException($"Invalid quantity for product {reqItem.ProductColorId}. Max allowed: {remaining}", 400);

						// Kiểm tra tồn kho Available tại Warehouse
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

						if (orderItem.StoreOrder.Status == StoreOrderStatus.Approved)
							orderItem.StoreOrder.Status = StoreOrderStatus.Processing;
					}

					// --- TRƯỜNG HỢP: GIAO KỆ (SHELF) ---
					else if (reqItem.ShelfTypeId != null || (reqItem.ShelfIds != null && reqItem.ShelfIds.Any()))
					{
						var isManual = reqItem.ShelfIds != null && reqItem.ShelfIds.Any();
						Guid shelfTypeId;
						List<Shelf> shelvesToReserve;

						if (isManual)
						{
							shelvesToReserve = await _shelfRepository.GetByIds(reqItem.ShelfIds!);
							if (shelvesToReserve.Count != reqItem.ShelfIds!.Count)
								throw new AppException("Some selected shelves were not found", 404);

							shelfTypeId = shelvesToReserve.First().ShelfTypeId;
						}
						else
						{
							shelfTypeId = reqItem.ShelfTypeId ?? throw new AppException("ShelfTypeId is required for Auto mode", 400);
							var qty = reqItem.ExpectedQuantity ?? throw new AppException("ExpectedQuantity is required for Auto mode", 400);

							shelvesToReserve = await _shelfRepository.GetAvailableShelvesByType(assignment.WarehouseLocationId, shelfTypeId, qty);
							if (shelvesToReserve.Count < qty)
								throw new AppException($"Not enough available shelves of type {shelfTypeId}", 400);
						}

						// Tìm item trong đơn hàng kệ đã gom
						var shelfOrderItem = relatedShelfOrders.SelectMany(x => x.Items)
							.FirstOrDefault(x => x.ShelfTypeId == shelfTypeId);

						if (shelfOrderItem == null)
							throw new AppException($"Shelf type {shelfTypeId} is not in assigned orders for this destination", 400);

						foreach (var shelf in shelvesToReserve)
						{
							if (shelf.Status != ShelfStatus.Available || shelf.InventoryLocationId != assignment.WarehouseLocationId)
								throw new AppException($"Shelf {shelf.Code} is not available at this warehouse", 400);

							shelf.Status = ShelfStatus.Reserved; // Giữ chỗ ngay khi tạo Draft

							await _shelfShipmentItemRepository.AddAsync(new ShelfShipmentItem
							{
								Id = Guid.NewGuid(),
								ShipmentId = shipment.Id,
								ShelfId = shelf.Id,
								Status = ShelfShipmentStatus.InTransit
							});
						}

						if (shelfOrderItem.ShelfOrder.Status == ShelfOrderStatus.Approved)
							shelfOrderItem.ShelfOrder.Status = ShelfOrderStatus.Processing;
					}
				}

				// 7. Lưu thay đổi xuống Database
				await _unitOfWork.SaveChangesAsync();

				// 8. Map và trả về kết quả
				var result = await _shipmentRepository.GetByIdWithDetailsAsync(shipment.Id);
				return MapToResponse(result!);
			}
			catch (AppException) { throw; }
			catch (Exception ex)
			{
				throw new AppException($"Internal Error: {ex.Message}", 500);
			}
		}

		public async Task PickupAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			// 1. Lấy thông tin Shipment kèm đầy đủ chi tiết để tránh N+1
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Draft)
				throw new AppException("Shipment is not in Draft status and cannot be picked up", 400);

			try
			{
				// 2. Lưu bằng chứng lấy hàng
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

				// 3. XỬ LÝ LOGIC TỒN KHO

				// --- TRƯỜNG HỢP A: SẢN PHẨM TỪ STORE ORDER ---
				// Chỉ lọc những Items có StoreOrderItemId (hàng mới đi giao)
				var storeOrderItems = shipment.Items.Where(x => x.StoreOrderItemId != null).ToList();
				if (storeOrderItems.Any())
				{
					foreach (var item in storeOrderItems)
					{
						// Vì StoreOrder bắt buộc có Product, dùng .Value an toàn
						var productColorId = item.ProductColorId ?? throw new AppException("ProductColorId is required for store items", 400);

						var inventoryAvailable = await _inventoryRepository
							.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.Available);

						if (inventoryAvailable == null || inventoryAvailable.Quantity < item.ExpectedQuantity)
							throw new AppException($"Not enough available stock for product {productColorId}", 400);

						// Trừ Available - Cộng InTransit
						inventoryAvailable.Quantity -= item.ExpectedQuantity;

						var inventoryTransit = await _inventoryRepository
							.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.InTransit);

						if (inventoryTransit == null)
						{
							inventoryTransit = new Inventory
							{
								Id = Guid.NewGuid(),
								InventoryLocationId = shipment.FromLocationId,
								ProductColorId = productColorId,
								Status = InventoryStatus.InTransit,
								Quantity = item.ExpectedQuantity
							};
							await _inventoryRepository.AddAsync(inventoryTransit);
						}
						else
						{
							inventoryTransit.Quantity += item.ExpectedQuantity;
						}

						await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
						{
							Id = Guid.NewGuid(),
							ProductColorId = productColorId,
							FromLocationId = shipment.FromLocationId,
							ToLocationId = shipment.ToLocationId,
							FromStatus = InventoryStatus.Available,
							ToStatus = InventoryStatus.InTransit,
							Quantity = item.ExpectedQuantity,
							ReferenceType = InventoryReferenceType.Shipment,
							ReferenceId = shipment.Id,
							CreatedAt = _dateTime.UtcNow
						});
					}
				}

				// --- TRƯỜNG HỢP B: KỆ (SHELF) ---
				// Xử lý thông qua bảng trung gian ShelfShipmentItems (Luồng giao kệ mới)
				if (shipment.ShelfShipmentItems != null && shipment.ShelfShipmentItems.Any())
				{
					foreach (var item in shipment.ShelfShipmentItems)
					{
						var shelf = await _shelfRepository.GetByIdAsync(item.ShelfId);
						if (shelf == null) throw new AppException($"Shelf {item.ShelfId} not found", 404);

						if (shelf.Status != ShelfStatus.Reserved)
							throw new AppException($"Shelf {shelf.Code} must be Reserved first", 400);

						shelf.Status = ShelfStatus.InTransit;

						await _shelfTransactionRepository.AddAsync(new ShelfTransaction
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
						});
					}
				}

				// --- TRƯỜNG HỢP C: THU HỒI HÀNG/KỆ HƯ HỎNG (DAMAGE REPORT) ---
				if (shipment.DamageReports != null && shipment.DamageReports.Any())
				{
					foreach (var report in shipment.DamageReports)
					{
						// Duyệt qua từng Item chi tiết trong tờ khai hỏng
						foreach (var reportItem in report.Items)
						{
							// 1. Nếu Item là SẢN PHẨM
							if (reportItem.DamageItemType == DamageItemType.Product && reportItem.ProductColorId.HasValue)
							{
								var productColorId = reportItem.ProductColorId.Value;
								var qty = reportItem.Quantity ?? 1; // Mặc định là 1 nếu không nhập

								// Trừ kho trạng thái Damaged tại Store
								var inventoryDamaged = await _inventoryRepository
									.GetAsync(report.InventoryLocationId, productColorId, InventoryStatus.Damaged);

								if (inventoryDamaged == null || inventoryDamaged.Quantity < qty)
									throw new AppException($"Insufficient damaged stock for product {productColorId} at source location", 400);

								inventoryDamaged.Quantity -= qty;

								// Cộng vào trạng thái InTransit tại Store (đang trên xe shipper)
								var inventoryTransit = await _inventoryRepository
									.GetAsync(report.InventoryLocationId, productColorId, InventoryStatus.InTransit);

								if (inventoryTransit == null)
								{
									inventoryTransit = new Inventory
									{
										Id = Guid.NewGuid(),
										InventoryLocationId = report.InventoryLocationId,
										ProductColorId = productColorId,
										Status = InventoryStatus.InTransit,
										Quantity = qty
									};
									await _inventoryRepository.AddAsync(inventoryTransit);
								}
								else
								{
									inventoryTransit.Quantity += qty;
								}

								// Log Transaction cho sản phẩm
								await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
								{
									Id = Guid.NewGuid(),
									ProductColorId = productColorId,
									FromLocationId = report.InventoryLocationId, // Từ Store
									ToLocationId = shipment.FromLocationId,      // Về Warehouse (Source của Shipment)
									FromStatus = InventoryStatus.Damaged,
									ToStatus = InventoryStatus.InTransit,
									Quantity = qty,
									ReferenceType = InventoryReferenceType.DamageReport,
									ReferenceId = report.Id,
									CreatedAt = _dateTime.UtcNow
								});
							}

							// 2. Nếu Item là KỆ
							else if (reportItem.DamageItemType == DamageItemType.Shelf && reportItem.ShelfId.HasValue)
							{
								var shelf = await _shelfRepository.GetByIdAsync(reportItem.ShelfId.Value);
								if (shelf == null) throw new AppException($"Shelf {reportItem.ShelfId} not found", 404);

								// Chuyển trạng thái kệ từ Damaged sang InTransit
								// (Giả sử bồ có thêm trạng thái Damaged trong ShelfStatus)
								var oldStatus = shelf.Status;
								shelf.Status = ShelfStatus.InTransit;

								// Log Transaction cho kệ
								await _shelfTransactionRepository.AddAsync(new ShelfTransaction
								{
									Id = Guid.NewGuid(),
									ShelfId = shelf.Id,
									FromLocationId = report.InventoryLocationId,
									ToLocationId = shipment.FromLocationId,
									FromStatus = oldStatus,
									ToStatus = ShelfStatus.InTransit,
									ReferenceType = ShelfReferenceType.DamageReport,
									ReferenceId = report.Id,
									CreatedAt = _dateTime.UtcNow
								});
							}
						}

						// Cập nhật trạng thái phiếu báo hỏng thành InTransit theo đúng Enum DamageStatus bồ đã định nghĩa
						report.Status = DamageStatus.InTransit;
					}
				}
				// 4. Cập nhật trạng thái Shipment
				shipment.Status = ShipmentStatus.Shipping;
				shipment.PickedUpAt = _dateTime.UtcNow;

				_shipmentRepository.Update(shipment);

				// 5. Lưu tất cả thay đổi vào Database
				await _unitOfWork.SaveChangesAsync();
			}
			catch (AppException) { throw; }
			catch (Exception ex)
			{
				throw new AppException($"An error occurred during pickup: {ex.Message}", 500);
			}
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
			// 1. Lấy thông tin Shipment kèm đầy đủ Details (Items, ShelfItems, DamageReports)
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Delivered)
				throw new AppException("Shipment must be in Delivered status to receive", 400);

			try
			{
				// ================= TRƯỜNG HỢP 1: STORE ORDER (GIAO SẢN PHẨM) =================
				if (shipment.StoreOrders != null && shipment.StoreOrders.Any())
				{
					foreach (var item in shipment.Items.Where(x => x.StoreOrderItemId != null))
					{
						// 1. Kiểm tra an toàn ProductColorId và Request
						if (!item.ProductColorId.HasValue) continue;

						var productColorId = item.ProductColorId.Value;
						var reqItem = request.ProductItems?.FirstOrDefault(x => x.ProductColorId == productColorId);

						if (reqItem == null) continue;

						var receivedQty = reqItem.ReceivedQuantity;
						var damagedQty = item.ExpectedQuantity - receivedQty; // Số lượng hỏng/mất do vận chuyển

						// Cập nhật số lượng thực nhận vào ShipmentItem
						item.ReceivedQuantity = receivedQty;

						// 2. Cập nhật số lượng hoàn thành cho đơn hàng gốc (StoreOrderItem)
						if (item.StoreOrderItem != null)
						{
							item.StoreOrderItem.FulfilledQuantity += receivedQty;
						}

						// 3. XỬ LÝ KHO NGUỒN (Trừ hàng đang đi đường - InTransit tại Warehouse)
						var warehouseTransit = await _inventoryRepository.GetAsync(
							shipment.FromLocationId,
							productColorId,
							InventoryStatus.InTransit);

						if (warehouseTransit != null)
						{
							// Trừ đúng số lượng shipper đã bốc đi ban đầu
							warehouseTransit.Quantity -= item.ExpectedQuantity;
						}
						else
						{
							// Nếu không tìm thấy hàng InTransit, có thể do dữ liệu lệch, cần cẩn thận ở đây
							throw new AppException($"Transit stock not found for product {productColorId} at warehouse", 400);
						}

						// 4. XỬ LÝ KHO ĐÍCH (Cộng hàng sẵn sàng sử dụng - Available tại Store)
						if (receivedQty > 0)
						{
							var storeAvailable = await _inventoryRepository.GetAsync(
								shipment.ToLocationId,
								productColorId,
								InventoryStatus.Available);

							if (storeAvailable == null)
							{
								storeAvailable = new Inventory
								{
									Id = Guid.NewGuid(),
									InventoryLocationId = shipment.ToLocationId,
									ProductColorId = productColorId,
									Status = InventoryStatus.Available,
									Quantity = 0
								};
								await _inventoryRepository.AddAsync(storeAvailable);
							}

							storeAvailable.Quantity += receivedQty;

							// Ghi log giao dịch: InTransit -> Available
							await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
							{
								Id = Guid.NewGuid(),
								ProductColorId = productColorId,
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

						// 5. XỬ LÝ HÀNG HƯ/MẤT (Đưa vào kho Damaged của Warehouse để xử lý bảo hiểm/kế toán)
						if (damagedQty > 0)
						{
							var warehouseDamaged = await _inventoryRepository.GetAsync(
								shipment.FromLocationId,
								productColorId,
								InventoryStatus.Damaged);

							if (warehouseDamaged == null)
							{
								warehouseDamaged = new Inventory
								{
									Id = Guid.NewGuid(),
									InventoryLocationId = shipment.FromLocationId,
									ProductColorId = productColorId,
									Status = InventoryStatus.Damaged,
									Quantity = 0
								};
								await _inventoryRepository.AddAsync(warehouseDamaged);
							}

							warehouseDamaged.Quantity += damagedQty;

							// Ghi log giao dịch: InTransit -> Damaged (Quay về kho nguồn xử lý lỗi vận chuyển)
							await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
							{
								Id = Guid.NewGuid(),
								ProductColorId = productColorId,
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

					// 6. CẬP NHẬT TRẠNG THÁI CHO TỪNG STORE ORDER (Tự động tính toán Fulfilled/Partial)
					foreach (var order in shipment.StoreOrders)
					{
						var totalOrdered = order.Items.Sum(x => x.Quantity);
						var totalFulfilled = order.Items.Sum(x => x.FulfilledQuantity);

						if (totalFulfilled >= totalOrdered)
						{
							order.Status = StoreOrderStatus.Fulfilled;
						}
						else if (totalFulfilled > 0)
						{
							order.Status = StoreOrderStatus.PartiallyFulfilled;
						}
						else
						{
							// Nếu đã giao nhưng không nhận được cái nào, giữ Approved để chờ giao lại
							order.Status = StoreOrderStatus.Approved;
						}
					}
				}

				// ================= LUỒNG 2: KỆ (SHELF ORDERS) =================

				if (shipment.ShelfShipmentItems != null && shipment.ShelfShipmentItems.Any())
				{
					foreach (var item in shipment.ShelfShipmentItems)
					{
						var reqShelf = request.ShelfItems?.FirstOrDefault(x => x.ShelfId == item.ShelfId);
						if (reqShelf == null) continue;

						var shelf = item.Shelf;
						if (reqShelf.IsReceived)
						{
							shelf.InventoryLocationId = shipment.ToLocationId;
							shelf.Status = ShelfStatus.InUse;
							shelf.AssignedAt = _dateTime.UtcNow;
							item.Status = ShelfShipmentStatus.Received;

							if (item.ShelfOrderItem != null) item.ShelfOrderItem.FulfilledQuantity += 1;
						}
						else
						{
							shelf.Status = ShelfStatus.Maintenance;
							item.Status = ShelfShipmentStatus.Damaged;
						}
					}

					foreach (var sOrder in shipment.ShelfOrders)
					{
						var totalOrdered = sOrder.Items.Sum(x => x.Quantity);
						var totalFulfilled = sOrder.Items.Sum(x => x.FulfilledQuantity);
						sOrder.Status = totalFulfilled >= totalOrdered ? ShelfOrderStatus.Fulfilled : (totalFulfilled > 0 ? ShelfOrderStatus.PartiallyFulfilled : ShelfOrderStatus.Approved);
					}
				}

				// ================= TRƯỜNG HỢP 3: DAMAGE REPORT (THU HỒI HÀNG HỎNG) =================
				// Đây là lúc hàng hỏng từ Store đã về đến Warehouse
				if (shipment.DamageReports != null && shipment.DamageReports.Any())
				{
					foreach (var report in shipment.DamageReports)
					{
						foreach (var reportItem in report.Items)
						{
							if (reportItem.DamageItemType == DamageItemType.Product && reportItem.ProductColorId.HasValue)
							{
								var productColorId = reportItem.ProductColorId.Value;
								var qty = reportItem.Quantity ?? 0;

								// Trừ InTransit (tại vị trí Store cũ) -> Cộng Damaged (tại Warehouse mới)
								var storeTransit = await _inventoryRepository.GetAsync(report.InventoryLocationId, productColorId, InventoryStatus.InTransit);
								if (storeTransit != null) storeTransit.Quantity -= qty;

								var warehouseDamaged = await _inventoryRepository.GetAsync(shipment.ToLocationId, productColorId, InventoryStatus.Damaged);
								if (warehouseDamaged == null)
								{
									warehouseDamaged = new Inventory { Id = Guid.NewGuid(), InventoryLocationId = shipment.ToLocationId, ProductColorId = productColorId, Status = InventoryStatus.Damaged, Quantity = 0 };
									await _inventoryRepository.AddAsync(warehouseDamaged);
								}
								warehouseDamaged.Quantity += qty;

								await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
								{
									Id = Guid.NewGuid(),
									ProductColorId = productColorId,
									FromLocationId = report.InventoryLocationId,
									ToLocationId = shipment.ToLocationId,
									FromStatus = InventoryStatus.InTransit,
									ToStatus = InventoryStatus.Damaged,
									Quantity = qty,
									ReferenceType = InventoryReferenceType.DamageReport,
									ReferenceId = report.Id,
									CreatedAt = _dateTime.UtcNow
								});
							}
						}
						report.Status = DamageStatus.Returned; // Kết thúc vòng đời Damage Report
					}
				}

				// 4. Chốt Shipment
				shipment.Status = ShipmentStatus.Received;
				shipment.ReceivedAt = _dateTime.UtcNow;

				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new AppException($"Receive failed: {ex.Message}", 500);
			}
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
			// Cần Load thêm Shelf và ShelfType để Map thông tin
			var shipment = await _shipmentRepository.GetByIdWithShelfItemsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			// Thay vì check StoreOrderId != null, ta check xem shipment này có thực sự chứa kệ không
			if (shipment.ShelfShipmentItems == null || !shipment.ShelfShipmentItems.Any())
				return new List<ShelfShipmentItemResponse>();

			var result = shipment.ShelfShipmentItems
				.Select(x => new ShelfShipmentItemResponse
				{
					ShelfId = x.ShelfId,
					Code = x.Shelf?.Code ?? "N/A",
					ShelfTypeName = x.Shelf?.ShelfType?.Name ?? "Unknown",
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
			// 1. Xác định loại vận đơn thông minh hơn
			string orderType = shipment.IsReturn ? "RETURN" : "STORE";
			if (!shipment.IsReturn)
			{
				bool hasStore = shipment.StoreOrders.Any();
				bool hasShelf = shipment.ShelfOrders.Any();

				if (hasStore && hasShelf) orderType = "MIXED";
				else if (hasShelf) orderType = "SHELF";
			}

			var response = new ShipmentResponse
			{
				Id = shipment.Id,
				Code = shipment.Code,
				IsReturn = shipment.IsReturn,
				OrderType = orderType,

				// Trả về danh sách IDs để FE có thể link tới chi tiết từng đơn con
				StoreOrderIds = shipment.StoreOrders.Select(x => x.Id).ToList(),
				ShelfOrderIds = shipment.ShelfOrders.Select(x => x.Id).ToList(),
				DamageReportIds = shipment.DamageReports.Select(x => x.Id).ToList(),

				FromLocationId = shipment.FromLocationId,
				FromLocationName = shipment.FromLocation?.Name ?? "Unknown",
				ToLocationId = shipment.ToLocationId,
				ToLocationName = shipment.ToLocation?.Name ?? "Unknown",

				// Shipper có thể lấy từ Assignment hoặc trực tiếp từ Shipment tùy vào bồ gán ở đâu
				ShipperName = shipment.Shipper?.FullName ?? shipment.ShipmentAssignment?.Shipper?.FullName,

				Status = shipment.Status,
				CreatedAt = shipment.CreatedAt,
				PickedUpAt = shipment.PickedUpAt,
				DeliveredAt = shipment.DeliveredAt,
				ReceivedAt = shipment.ReceivedAt
			};

			// ================= MAPPING SẢN PHẨM =================
			if (shipment.Items != null && shipment.Items.Any(x => x.ProductColorId.HasValue))
			{
				response.ProductItems = shipment.Items
					.Where(x => x.ProductColorId.HasValue)
					.Select(x => new ShipmentProductItemResponse
					{
						ProductColorId = x.ProductColorId!.Value,
						SKU = x.ProductColor?.Product?.SKU ?? "N/A",
						ProductName = x.ProductColor?.Product?.Name ?? "Unknown",
						Color = x.ProductColor?.Color?.Name ?? "N/A",
						ImageUrl = x.ProductColor?.ImageUrl,
						ExpectedQuantity = x.ExpectedQuantity,
						ReceivedQuantity = x.ReceivedQuantity
					}).ToList();
			}

			// ================= MAPPING KỆ =================
			// Gom nhóm kệ theo ShelfType để hiển thị gọn gàng (Ví dụ: 5 kệ loại A, 2 kệ loại B)
			if (shipment.ShelfShipmentItems != null && shipment.ShelfShipmentItems.Any())
			{
				response.ShelfItems = shipment.ShelfShipmentItems
					.Where(x => x.Shelf != null)
					.GroupBy(x => x.Shelf!.ShelfTypeId)
					.Select(g =>
					{
						var firstItem = g.First();
						var shelfType = firstItem.Shelf?.ShelfType;

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
