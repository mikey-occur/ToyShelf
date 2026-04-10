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
			// 1. Validation & Load Data
			if (request.Items == null || !request.Items.Any())
				throw new AppException("Shipment items are required", 400);

			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);
			if (assignment == null) throw new AppException("Assignment not found", 404);

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment before creating shipment", 400);

			if (assignment.Status == AssignmentStatus.Rejected)
				throw new AppException("Shipper has rejected this assignment", 400);

			if (assignment.ShipperId == null)
				throw new AppException("Shipper not assigned to this assignment", 400);


			// Xác định đích đến (ToLocation) dựa trên đơn hàng đầu tiên tìm thấy trong assignment
			var toLocationId = assignment.AssignmentStoreOrders.Select(x => (Guid?)x.StoreOrder.StoreLocationId).FirstOrDefault()
							?? assignment.AssignmentShelfOrders.Select(x => (Guid?)x.ShelfOrder.StoreLocationId).FirstOrDefault()
							?? assignment.AssignmentDamageReports.Select(x => (Guid?)x.DamageReport.InventoryLocationId).FirstOrDefault();

			if (toLocationId == null) throw new AppException("Destination location could not be determined", 400);

			var code = await GenerateCode();
			// 2. Khởi tạo Shipment Header
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
				CreatedAt = _dateTime.UtcNow,
				IsReturn = false // Mặc định là false, sẽ update nếu có DamageReport thu hồi
			};

			// 3. Duyệt danh sách Item do WM gửi lên
			foreach (var reqItem in request.Items)
			{
				//  TRƯỜNG HỢP: SẢN PHẨM (STORE ORDER) 
				if (reqItem.StoreOrderId.HasValue && reqItem.ProductColorId.HasValue)
				{
					var order = assignment.AssignmentStoreOrders
						.Where(x => x.StoreOrderId == reqItem.StoreOrderId)
						.Select(x => x.StoreOrder).FirstOrDefault();

					if (order == null) throw new AppException($"Store Order {reqItem.StoreOrderId} not in this assignment", 400);

					var orderItem = order.Items.FirstOrDefault(i => i.ProductColorId == reqItem.ProductColorId);
					if (orderItem == null) throw new AppException("Product not found in the specified store order", 400);

					// Kiểm tra số lượng WM bốc so với yêu cầu còn lại
					var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;
					if (reqItem.ExpectedQuantity <= 0 || reqItem.ExpectedQuantity > remaining)
						throw new AppException($"Invalid qty for {order.Code}. Max allowed: {remaining}", 400);

					// Kiểm tra tồn kho Available tại Warehouse
					var inventory = await _inventoryRepository.GetAsync(assignment.WarehouseLocationId, reqItem.ProductColorId.Value, InventoryStatus.Available);
					if (inventory == null || inventory.Quantity < reqItem.ExpectedQuantity)
						throw new AppException($"Out of stock for {orderItem.ProductColor.Product.Name} in warehouse", 400);

					// Add Item vào Shipment (Link tới StoreOrderItemId chuẩn xác)
					shipment.Items.Add(new ShipmentItem
					{
						Id = Guid.NewGuid(),
						StoreOrderItemId = orderItem.Id,
						ProductColorId = reqItem.ProductColorId,
						ExpectedQuantity = reqItem.ExpectedQuantity,
						ReceivedQuantity = 0
					});

					if (!shipment.StoreOrders.Contains(order)) shipment.StoreOrders.Add(order);
					order.Status = StoreOrderStatus.Processing;
				}

				// TRƯỜNG HỢP: KỆ (SHELF ORDER) 
				else if (reqItem.ShelfOrderId.HasValue && (reqItem.ShelfTypeId.HasValue || reqItem.ShelfIds?.Any() == true))
				{
					var sOrder = assignment.AssignmentShelfOrders
						.Where(x => x.ShelfOrderId == reqItem.ShelfOrderId)
						.Select(x => x.ShelfOrder).FirstOrDefault();

					if (sOrder == null) throw new AppException("Shelf Order not found", 400);

					var shelfOrderItem = sOrder.Items.FirstOrDefault(i => i.ShelfTypeId == reqItem.ShelfTypeId);
					if (shelfOrderItem == null) throw new AppException("Shelf type not in this order", 400);

					// Xử lý chọn kệ: Auto (theo Type) hoặc Manual (theo danh sách Id)
					List<Shelf> shelvesToReserve;
					if (reqItem.ShelfIds != null && reqItem.ShelfIds.Any())
					{
						shelvesToReserve = await _shelfRepository.GetByIds(reqItem.ShelfIds);
						if (shelvesToReserve.Count != reqItem.ShelfIds.Count) throw new AppException("Some shelves not found", 404);
					}
					else
					{
						shelvesToReserve = await _shelfRepository.GetAvailableShelvesByType(
							assignment.WarehouseLocationId, reqItem.ShelfTypeId!.Value, reqItem.ExpectedQuantity);
					}

					if (shelvesToReserve.Count < reqItem.ExpectedQuantity)
						throw new AppException($"Not enough available shelves for {sOrder.Code}", 400);

					// Add từng con kệ vào ShelfShipmentItems
					foreach (var shelf in shelvesToReserve)
					{
						shelf.Status = ShelfStatus.Reserved; // Lock kệ ngay lập tức
						shipment.ShelfShipmentItems.Add(new ShelfShipmentItem
						{
							Id = Guid.NewGuid(),
							ShelfId = shelf.Id,
							ShelfOrderItemId = shelfOrderItem.Id, // Link nguồn gốc kệ
							Status = ShelfShipmentStatus.InTransit
						});
					}

					if (!shipment.ShelfOrders.Contains(sOrder)) shipment.ShelfOrders.Add(sOrder);
					sOrder.Status = ShelfOrderStatus.Processing;
				}
			}

			// 4. TRƯỜNG HỢP: THU HỒI (DAMAGE REPORT) 
			// Hệ thống sẽ tự quét các DamageReport của ShipmentAsssignment thông quá AssignmentDamageReports
			var pendingDamageReports = assignment.AssignmentDamageReports
				.Select(x => x.DamageReport)
				.Where(d => d.Status == DamageStatus.Approved) // Chỉ lấy những đơn đã được Admin Duyệt
				.ToList();

			foreach (var report in pendingDamageReports)
			{
				// Chuyển trạng thái sang "Đã gán xe/Đang chờ lấy"
				report.Status = DamageStatus.Scheduled;

				// Liên kết DamageReport vào Shipment
				if (!shipment.DamageReports.Contains(report))
					shipment.DamageReports.Add(report);

				foreach (var dItem in report.Items)
				{
					// Tự động map toàn bộ hàng hỏng vào ShipmentItems
					shipment.Items.Add(new ShipmentItem
					{
						Id = Guid.NewGuid(),
						ShipmentId = shipment.Id,
						DamageReportItemId = dItem.Id,
						ProductColorId = dItem.ProductColorId,
						ShelfId = dItem.ShelfId,
						// Kệ hỏng mặc định là 1, sản phẩm lấy theo số lượng báo cáo
						ExpectedQuantity = dItem.DamageItemType == DamageItemType.Shelf ? 1 : (dItem.Quantity ?? 0),
						ReceivedQuantity = 0
					});
				}
			}

			// 4. Lưu và Trả về
			await _shipmentRepository.AddAsync(shipment);
			await _unitOfWork.SaveChangesAsync();

			var result = await _shipmentRepository.GetByIdWithDetailsAsync(shipment.Id);
			return MapToResponse(result!);
		}
		public async Task PickupAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null) throw new AppException("Shipment not found", 404);
			if (shipment.Status != ShipmentStatus.Draft)
				throw new AppException("Shipment is not in Draft status and cannot be picked up", 400);

			try
			{
				// 1. Lưu bằng chứng lấy hàng tại kho
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

				// 2. XỬ LÝ HÀNG MỚI (STORE ORDER) - Trừ Available tại Warehouse
				var storeOrderItems = shipment.Items.Where(x => x.StoreOrderItemId != null).ToList();
				foreach (var item in storeOrderItems)
				{
					var productColorId = item.ProductColorId!.Value;

					var inventoryAvailable = await _inventoryRepository.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.Available);
					if (inventoryAvailable == null || inventoryAvailable.Quantity < item.ExpectedQuantity)
						throw new AppException($"Not enough available stock for product {productColorId} at warehouse", 400);

					inventoryAvailable.Quantity -= item.ExpectedQuantity;

					// Chuyển sang InTransit của Warehouse
					var inventoryTransit = await _inventoryRepository.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.InTransit);
					if (inventoryTransit == null)
					{
						inventoryTransit = new Inventory { Id = Guid.NewGuid(), InventoryLocationId = shipment.FromLocationId, ProductColorId = productColorId, Status = InventoryStatus.InTransit, Quantity = item.ExpectedQuantity };
						await _inventoryRepository.AddAsync(inventoryTransit);
					}
					else inventoryTransit.Quantity += item.ExpectedQuantity;

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

				// 3. XỬ LÝ KỆ MỚI (SHELF ORDER) - Reserved -> InTransit
				foreach (var item in shipment.ShelfShipmentItems)
				{
					var shelf = await _shelfRepository.GetByIdAsync(item.ShelfId);
					if (shelf == null || shelf.Status != ShelfStatus.Reserved)
						throw new AppException($"Shelf {shelf?.Code} is not ready (Reserved) for pickup", 400);

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

				// 4. Update Header
				shipment.Status = ShipmentStatus.Shipping;
				shipment.PickedUpAt = _dateTime.UtcNow;

				_shipmentRepository.Update(shipment);
				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception)
			{
				// Giữ nguyên Stack Trace gốc để biết chính xác lỗi ở dòng nào
				throw;
			}
		}
		public async Task PickupReturnAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null) throw new AppException("Shipment not found", 404);
			// Chỉ cho phép thu hồi khi xe đang trong hành trình giao hoặc đã giao xong 1 phần
			if (shipment.Status != ShipmentStatus.Shipping && shipment.Status != ShipmentStatus.Delivered)
				throw new AppException("Invalid status to pickup return items", 400);

			try
			{
				// 1. Lưu bằng chứng bốc hàng hỏng tại Store
				var media = new ShipmentMedia
				{
					Id = Guid.NewGuid(),
					ShipmentId = shipmentId,
					UploadedByUserId = currentUser.UserId,
					MediaUrl = request.MediaUrl,
					MediaType = ShipmentMediaType.Image,
					Purpose = ShipmentMediaPurpose.ReturnPickup, // Enum mới cho thu hồi
					CreatedAt = _dateTime.UtcNow
				};
				await _shipmentMediaRepository.AddAsync(media);

				// 2. Duyệt qua các DamageReport được gán trong Shipment
				foreach (var report in shipment.DamageReports)
				{
					if (report.Status != DamageStatus.Scheduled) continue;

					foreach (var reportItem in report.Items)
					{
						// TRƯỜNG HỢP: SẢN PHẨM HỎNG - Trừ Damaged tại Store -> Cộng InTransit tại Store
						if (reportItem.DamageItemType == DamageItemType.Product && reportItem.ProductColorId.HasValue)
						{
							var qty = reportItem.Quantity ?? 0;
							var invDamaged = await _inventoryRepository.GetAsync(report.InventoryLocationId, reportItem.ProductColorId.Value, InventoryStatus.Damaged);

							if (invDamaged == null || invDamaged.Quantity < qty)
								throw new AppException($"Store does not have enough damaged stock for {reportItem.ProductColorId}", 400);

							invDamaged.Quantity -= qty;

							var invTransit = await _inventoryRepository.GetAsync(report.InventoryLocationId, reportItem.ProductColorId.Value, InventoryStatus.InTransit);
							if (invTransit == null)
							{
								invTransit = new Inventory { Id = Guid.NewGuid(), InventoryLocationId = report.InventoryLocationId, ProductColorId = reportItem.ProductColorId.Value, Status = InventoryStatus.InTransit, Quantity = qty };
								await _inventoryRepository.AddAsync(invTransit);
							}
							else invTransit.Quantity += qty;

							await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
							{
								Id = Guid.NewGuid(),
								ProductColorId = reportItem.ProductColorId.Value,
								FromLocationId = report.InventoryLocationId, // Từ Store
								ToLocationId = shipment.FromLocationId,      // Đích là Warehouse
								FromStatus = InventoryStatus.Damaged,
								ToStatus = InventoryStatus.InTransit,
								Quantity = qty,
								ReferenceType = InventoryReferenceType.DamageReport,
								ReferenceId = report.Id,
								CreatedAt = _dateTime.UtcNow
							});
						}
						// TRƯỜNG HỢP: KỆ HỎNG - Chuyển trạng thái kệ tại Store sang InTransit
						else if (reportItem.DamageItemType == DamageItemType.Shelf && reportItem.ShelfId.HasValue)
						{
							var shelf = await _shelfRepository.GetByIdAsync(reportItem.ShelfId.Value);
							if (shelf == null)
								throw new AppException($"Shelf with ID {reportItem.ShelfId} not found", 404);
							var oldStatus = shelf.Status; // Thường là Maintenance hoặc Damaged
							shelf.Status = ShelfStatus.InTransit;

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
					report.Status = DamageStatus.InTransit;
				}

				// 3. Update Header
				shipment.IsReturn = true; // Chính thức xác nhận xe đang chở hàng về

				_shipmentRepository.Update(shipment);
				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception)
			{
				// Giữ nguyên Stack Trace gốc để biết chính xác lỗi ở dòng nào
				throw;
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

		//public async Task ArrivedWarehouseAsync(Guid shipmentId, ICurrentUser currentUser)
		//{
		//	var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);

		//	if (shipment == null) throw new AppException("Shipment not found", 404);

		//	// Chỉ ghi nhận khi xe đang trên đường về (IsReturn đã bật)
		//	if (!shipment.IsReturn)
		//		throw new AppException("Shipment is not in return process", 400);

		//	shipment.ArrivedWarehouseAt = _dateTime.UtcNow;
		//	// Bồ cần thêm field ArrivedWarehouseAt vào DB tương tự DeliveredAt

		//	_shipmentRepository.Update(shipment);
		//	await _unitOfWork.SaveChangesAsync();
		//}

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
