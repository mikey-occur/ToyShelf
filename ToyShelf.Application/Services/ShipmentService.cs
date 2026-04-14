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
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IShelfOrderRepository _shelfOrderRepository;
		private readonly IInventoryShelfRepository _inventoryShelfRepository; 
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
			IStoreOrderRepository storeOrderRepository,
			IShelfOrderRepository shelfOrderRepository,
			IInventoryShelfRepository inventoryShelfRepository,
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
			_storeOrderRepository = storeOrderRepository;
			_shelfOrderRepository = shelfOrderRepository;
			_inventoryShelfRepository = inventoryShelfRepository;
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
			// Kiểm tra xem có bất kỳ mặt hàng nào (sản phẩm hoặc kệ) được gửi lên không
			if (!request.Products.Any() && !request.Shelves.Any())
				throw new AppException("Shipment must contain at least one product or shelf", 400);

			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);
			if (assignment == null) throw new AppException("Assignment not found", 404);

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment before creating shipment", 400);

			if (assignment.ShipperId == null)
				throw new AppException("Shipper not assigned to this assignment", 400);

			// Xác định đích đến (ToLocation)
			var toLocationId = assignment.AssignmentStoreOrders.Select(x => (Guid?)x.StoreOrder.StoreLocationId).FirstOrDefault()
							?? assignment.AssignmentShelfOrders.Select(x => (Guid?)x.ShelfOrder.StoreLocationId).FirstOrDefault()
							?? assignment.AssignmentDamageReports.Select(x => (Guid?)x.DamageReport.InventoryLocationId).FirstOrDefault();

			if (toLocationId == null) throw new AppException("Destination location could not be determined", 400);

			var code = await GenerateCode();
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
				IsReturn = assignment.AssignmentDamageReports?.Any() ?? false
			};

			// 2. XỬ LÝ SẢN PHẨM (Store Orders)
			foreach (var pReq in request.Products)
			{
				var order = assignment.AssignmentStoreOrders
					.Where(x => x.StoreOrderId == pReq.StoreOrderId)
					.Select(x => x.StoreOrder).FirstOrDefault();

				if (order == null) throw new AppException($"Order {pReq.StoreOrderId} not in assignment", 400);

				var orderItem = order.Items.FirstOrDefault(i => i.ProductColorId == pReq.ProductColorId);
				if (orderItem == null) throw new AppException("Product not found in store order", 400);

				// Check số lượng hợp lệ
				var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;
				if (pReq.ExpectedQuantity <= 0 || pReq.ExpectedQuantity > remaining)
					throw new AppException($"Invalid qty for {order.Code}. Max: {remaining}", 400);

				// Check tồn kho Available tại Warehouse
				var inventory = await _inventoryRepository.GetAsync(assignment.WarehouseLocationId, pReq.ProductColorId, InventoryStatus.Available);
				if (inventory == null || inventory.Quantity < pReq.ExpectedQuantity)
					throw new AppException($"Out of stock for {orderItem.ProductColor?.Product?.Name}", 400);

				shipment.Items.Add(new ShipmentItem
				{
					Id = Guid.NewGuid(),
					StoreOrderItemId = orderItem.Id,
					ProductColorId = pReq.ProductColorId,
					ExpectedQuantity = pReq.ExpectedQuantity,
					ReceivedQuantity = 0
				});

				if (!shipment.StoreOrders.Contains(order)) shipment.StoreOrders.Add(order);
				order.Status = StoreOrderStatus.Processing;
			}

			// 3. XỬ LÝ KỆ (Shelf Orders)
			foreach (var sReq in request.Shelves)
			{
				var sOrder = assignment.AssignmentShelfOrders
					.Where(x => x.ShelfOrderId == sReq.ShelfOrderId)
					.Select(x => x.ShelfOrder).FirstOrDefault();

				if (sOrder == null) throw new AppException("Shelf Order not found", 400);

				var shelfOrderItem = sOrder.Items.FirstOrDefault(i => i.ShelfTypeId == sReq.ShelfTypeId);
				if (shelfOrderItem == null) throw new AppException("Shelf type not in this order", 400);

				// --- LOGIC DỊCH CHUYỂN TỒN KHO TRÊN SỔ SÁCH (InventoryShelf) ---

				// 1. Tìm bản ghi Available để trừ số lượng
				var invAvailable = await _inventoryShelfRepository.GetShelfWithStatusAsync(
					assignment.WarehouseLocationId, sReq.ShelfTypeId, ShelfStatus.Available);

				if (invAvailable == null || !invAvailable.HasEnoughStock(sReq.ExpectedQuantity))
					throw new AppException($"Kho không đủ kệ sẵn dùng (Available). Loại: {shelfOrderItem.ShelfType?.Name}", 400);

				// THỰC HIỆN TRỪ: Lấy từ ngăn "Available"
				invAvailable.RemoveQuantity(sReq.ExpectedQuantity);

				// 2. Tìm bản ghi Reserved để cộng số lượng (Theo dõi hàng đang chuẩn bị đi)
				var invReserved = await _inventoryShelfRepository.GetShelfWithStatusAsync(
					assignment.WarehouseLocationId, sReq.ShelfTypeId, ShelfStatus.Reserved);

				if (invReserved == null)
				{
					// Nếu chưa có "ngăn" Reserved cho loại kệ này tại kho này -> Tạo mới bản ghi InventoryShelf
					// Giả sử Constructor InventoryShelf của bạn nhận thêm tham số Status
					invReserved = new InventoryShelf(
						assignment.WarehouseLocationId,
						sReq.ShelfTypeId,
						sReq.ExpectedQuantity,
						ShelfStatus.Reserved
					);
					await _inventoryShelfRepository.AddAsync(invReserved);
				}
				else
				{
					// Nếu đã có bản ghi Reserved -> Cộng thêm vào
					invReserved.AddQuantity(sReq.ExpectedQuantity);
				}

				// --- LOGIC XỬ LÝ ĐỊNH DANH TỦ VẬT LÝ (Shelf) ---
				List<Shelf> shelvesToReserve;
				if (sReq.ShelfIds != null && sReq.ShelfIds.Any())
				{
					shelvesToReserve = await _shelfRepository.GetByIds(sReq.ShelfIds);
					if (shelvesToReserve.Count != sReq.ShelfIds.Count)
						throw new AppException("Một số kệ vật lý được chọn không tồn tại", 404);

					if (shelvesToReserve.Any(x => x.Status != ShelfStatus.Available || x.InventoryLocationId != assignment.WarehouseLocationId))
						throw new AppException("Một số kệ đã chọn không khả dụng hoặc không nằm tại kho này", 400);
				}
				else
				{
					// Tự động lấy các kệ Available theo số lượng yêu cầu
					shelvesToReserve = await _shelfRepository.GetAvailableShelvesByType(
						assignment.WarehouseLocationId, sReq.ShelfTypeId, sReq.ExpectedQuantity);
				}

				if (shelvesToReserve.Count < sReq.ExpectedQuantity)
					throw new AppException($"Không đủ {sReq.ExpectedQuantity} kệ vật lý Available trong kho để gán định danh.", 400);

				// Cập nhật trạng thái từng cái tủ
				foreach (var shelf in shelvesToReserve)
				{
					shelf.Status = ShelfStatus.Reserved; // Đồng bộ với InventoryShelf.Status

					shipment.ShelfShipmentItems.Add(new ShelfShipmentItem
					{
						Id = Guid.NewGuid(),
						ShelfId = shelf.Id,
						ShelfOrderItemId = shelfOrderItem.Id,
						Status = ShelfShipmentStatus.InTransit // Status của dòng Item trong vận đơn
					});
				}

				// Cập nhật trạng thái đơn kệ
				if (!shipment.ShelfOrders.Contains(sOrder)) shipment.ShelfOrders.Add(sOrder);
				sOrder.Status = ShelfOrderStatus.Processing;
			}

			// 4. TỰ ĐỘNG XỬ LÝ THU HỒI (Damage Reports)
			// Thêm dấu ? và ?? để nếu không có DamageReport thì nó trả về list rỗng, foreach sẽ tự bỏ qua
			var pendingDamageReports = assignment.AssignmentDamageReports?
				.Select(x => x.DamageReport)
				.Where(d => d != null && d.Status == DamageStatus.Approved)
				.ToList() ?? new List<DamageReport>();

			foreach (var report in pendingDamageReports)
			{
				report.Status = DamageStatus.Scheduled;

				// Check trùng để an tâm, dù logic Assignment thường chỉ có 1 quan hệ 1-1 với Report
				if (!shipment.DamageReports.Contains(report))
					shipment.DamageReports.Add(report);

				foreach (var dItem in report.Items ?? new List<DamageReportItem>())
				{
					shipment.Items.Add(new ShipmentItem
					{
						Id = Guid.NewGuid(),
						// Link vào Shipment hiện tại
						ShipmentId = shipment.Id,
						DamageReportItemId = dItem.Id,
						ProductColorId = dItem.ProductColorId,
						ShelfId = dItem.ShelfId,
						// Logic: Kệ là 1, Sản phẩm lấy theo Quantity báo cáo
						ExpectedQuantity = dItem.DamageItemType == DamageItemType.Shelf ? 1 : (dItem.Quantity ?? 0),
						ReceivedQuantity = 0
					});
				}
			}

			// 5. Save & Response
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
				shipment.Status = ShipmentStatus.ShippingReturn; // Trạng thái mới cho luồng thu hồi
				shipment.ReturnPickedUpAt = _dateTime.UtcNow;

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
		public async Task ArrivedWarehouseAsync(Guid shipmentId, ICurrentUser currentUser)
		{
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);
			if (shipment == null) throw new AppException("Shipment not found", 404);

			// 1. Ghi nhận mốc thời gian về đến cổng kho cho cả 2 trường hợp
			shipment.ArrivedWarehouseAt = _dateTime.UtcNow;

			// 2. Phân loại trạng thái dựa trên việc có hàng thu hồi hay không
			if (!shipment.IsReturn)
			{
				// TRƯỜNG HỢP 1: KHÔNG CÓ HÀNG HỎNG
				// Xe về không -> Đóng đơn luôn, Shipper xong nhiệm vụ
				shipment.Status = ShipmentStatus.Completed;
			}
			else
			{
				// TRƯỜNG HỢP 2: CÓ HÀNG HỎNG
				// Chuyển sang DeliveredReturn để "treo" đơn đó lại
				// Ép Admin/WM phải vào hàm ReceiveReturn để xác nhận rồi mới cho Complete
				shipment.Status = ShipmentStatus.DeliveredReturn;
			}

			_shipmentRepository.Update(shipment);
			await _unitOfWork.SaveChangesAsync();
		}
		public async Task StoreReceiveAsync(Guid shipmentId, StoreReceiveRequest request)
		{
			// 1. Lấy thông tin Shipment kèm đầy đủ liên kết để xử lý lũy kế
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);
			if (shipment == null) throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Delivered)
				throw new AppException("Vận đơn chưa ở trạng thái Delivered để nhận hàng", 400);

			try
			{
				// ================= XỬ LÝ SẢN PHẨM =================
				if (request.ProductItems != null)
				{
					foreach (var req in request.ProductItems)
					{
						// Tìm chính xác dòng hàng dựa trên ShipmentItemId từ FE gửi lên
						var sItem = shipment.Items.FirstOrDefault(x => x.Id == req.ShipmentItemId);
						if (sItem == null || !sItem.ProductColorId.HasValue) continue;

						var productColorId = sItem.ProductColorId.Value;
						var actual = req.ReceivedQuantity;
						var expected = sItem.ExpectedQuantity;
						var missingOrDamaged = expected - actual;

						// Cập nhật thực nhận trên chuyến xe
						sItem.ReceivedQuantity = actual;

						// CỘNG DỒN VÀO ORDER GỐC (Lũy kế hoàn thành đơn hàng)
						if (sItem.StoreOrderItem != null)
						{
							sItem.StoreOrderItem.FulfilledQuantity += actual;
						}

						// TỒN KHO: A. Trừ hàng đang đi đường (InTransit) tại Warehouse nguồn
						// Trừ đúng số lượng shipper đã bốc đi ban đầu (Expected)
						var transit = await _inventoryRepository.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.InTransit);
						if (transit != null)
						{
							transit.Quantity -= expected;
						}

						// TỒN KHO: B. Cộng vào hàng sẵn sàng (Available) tại Store đích
						await HandleInventoryUpdateAsync(
							shipment.FromLocationId, shipment.ToLocationId, productColorId,
							InventoryStatus.InTransit, InventoryStatus.Available,
							actual, shipment.Id, InventoryReferenceType.Shipment);

						// TỒN KHO: C. Nếu có hỏng/mất, đưa vào kho Damaged của Warehouse nguồn để xử lý shipper
						if (missingOrDamaged > 0)
						{
							await HandleInventoryUpdateAsync(
								shipment.FromLocationId, shipment.FromLocationId, productColorId,
								InventoryStatus.InTransit, InventoryStatus.Damaged,
								missingOrDamaged, shipment.Id, InventoryReferenceType.Shipment);
						}
					}
				}

				// ================= XỬ LÝ KỆ (SHELF) =================
				if (request.ShelfItems != null)
				{
					foreach (var reqShelf in request.ShelfItems)
					{
						var sItem = shipment.ShelfShipmentItems.FirstOrDefault(x => x.Id == reqShelf.ShelfShipmentItemId);
						if (sItem == null || sItem.Shelf == null) continue;

						if (reqShelf.IsReceived)
						{
							sItem.Shelf.InventoryLocationId = shipment.ToLocationId;
							sItem.Shelf.Status = ShelfStatus.InUse;
							sItem.Status = ShelfShipmentStatus.Received;
							// Cộng dồn cho ShelfOrder gốc
							if (sItem.ShelfOrderItem != null) sItem.ShelfOrderItem.FulfilledQuantity += 1;
						}
						else
						{
							sItem.Shelf.Status = ShelfStatus.Maintenance;
							sItem.Status = ShelfShipmentStatus.Damaged;
						}
					}
				}

				// CẬP NHẬT TRẠNG THÁI SHELF ORDER TỔNG
				var relatedShelfOrderIds = shipment.ShelfShipmentItems
					.Select(x => x.ShelfOrderItem?.ShelfOrderId)
					.Where(id => id.HasValue)
					.Distinct();

				foreach (var shelfOrderId in relatedShelfOrderIds)
				{
					var sOrder = await _shelfOrderRepository.GetByIdAsync(shelfOrderId!.Value);
					if (sOrder != null)
					{
						// Kiểm tra nếu tất cả loại kệ trong đơn đã giao đủ số lượng
						bool isFull = sOrder.Items.All(i => i.FulfilledQuantity >= i.Quantity);

						if (isFull)
						{
							sOrder.Status = ShelfOrderStatus.Fulfilled;
						}
						else if (sOrder.Items.Any(i => i.FulfilledQuantity > 0))
						{
							sOrder.Status = ShelfOrderStatus.PartiallyFulfilled;
						}
					}
				}

				// CẬP NHẬT TRẠNG THÁI ORDER TỔNG 
				// Lấy danh sách các StoreOrder liên quan đến chuyến xe này
				var relatedOrderIds = shipment.Items
					.Select(x => x.StoreOrderItem?.StoreOrderId)
					.Where(id => id.HasValue)
					.Distinct();

				foreach (var orderId in relatedOrderIds)
				{
					var order = await _storeOrderRepository.GetByIdAsync(orderId!.Value);
					if (order != null)
					{
						// Kiểm tra nếu tất cả Item trong đơn hàng đã đủ số lượng
						bool isFull = order.Items.All(i => i.FulfilledQuantity >= i.Quantity);
						bool isPartial = order.Items.Any(i => i.FulfilledQuantity > 0);

						order.Status = isFull ? StoreOrderStatus.Fulfilled :
									  (isPartial ? StoreOrderStatus.PartiallyFulfilled : order.Status);
					}
				}

				// Đánh dấu thời điểm nhận hàng
				shipment.StoreReceivedAt = _dateTime.UtcNow;

				// Lưu toàn bộ thay đổi (Unit of Work)
				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new AppException($"Store receiving failed: {ex.Message}", 500);
			}
		}
		public async Task WarehouseReceiveReturnAsync(Guid shipmentId)
		{
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);
			if (shipment == null) throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.DeliveredReturn)
				throw new AppException("Vận đơn chưa ở trạng thái chờ nhận hàng trả (DeliveredReturn)", 400);

			try
			{
				// ================= XỬ LÝ THU HỒI HÀNG HỎNG (DAMAGE REPORTS) =================
				if (shipment.DamageReports != null)
				{
					foreach (var report in shipment.DamageReports)
					{
						foreach (var rItem in report.Items)
						{
							if (!rItem.ProductColorId.HasValue) continue;

							var productColorId = rItem.ProductColorId.Value;
							var qty = rItem.Quantity ?? 0;

							// 1. Trừ InTransit tại địa điểm Store (nơi shipper bốc hàng hỏng lên)
							var transit = await _inventoryRepository.GetAsync(report.InventoryLocationId, productColorId, InventoryStatus.InTransit);
							if (transit != null) transit.Quantity -= qty;

							// 2. Cộng vào kho Damaged tại Warehouse tổng (Xác nhận thu hồi xong)
							await HandleInventoryUpdateAsync(
								report.InventoryLocationId, shipment.FromLocationId, productColorId,
								InventoryStatus.InTransit, InventoryStatus.Damaged,
								qty, report.Id, InventoryReferenceType.DamageReport);
						}
						report.Status = DamageStatus.Returned; // Kết thúc Damage Report
					}
				}

				// ================= KẾT THÚC VÒNG ĐỜI VẬN ĐƠN =================
				shipment.Status = ShipmentStatus.Completed;
				shipment.WarehouseReceivedAt = _dateTime.UtcNow;

				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new AppException($"Warehouse return receiving failed: {ex.Message}", 500);
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

		// Hỗ trợ StoreReceiveAsync để FE có thể lấy chi tiết từng Item trong đơn hàng khi nhận hàng tại Store
		public async Task<ShipmentReceiveViewModel> GetShipmentForReceivingAsync(Guid shipmentId)
		{
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null) throw new AppException("Shipment not found", 404);

			// Chỉ cho phép lấy data nếu vận đơn đang trên đường giao (để tránh nhận ghi đè)
			if (shipment.Status != ShipmentStatus.Delivered)
				throw new AppException("Vận đơn không ở trạng thái có thể nhận hàng", 400);

			return new ShipmentReceiveViewModel
			{
				ShipmentId = shipment.Id,
				ShipmentCode = shipment.Code,
				FromLocationName = shipment.FromLocation?.Name ?? "N/A",
				ToLocationName = shipment.ToLocation?.Name ?? "N/A",

				// 1. Danh sách sản phẩm: FE lặp cái này ra các dòng Input
				ProductItems = shipment.Items
					.Where(x => x.StoreOrderItemId != null) // Lọc các dòng thuộc đơn hàng Store
					.Select(x => new ShipmentProductItemDto
					{
						ShipmentItemId = x.Id,
						ProductColorId = x.ProductColorId ?? Guid.Empty,
						ProductName = x.ProductColor?.Product?.Name ?? "Unknown",
						ColorName = x.ProductColor?.Color?.Name ?? "N/A",
						ImageUrl = x.ProductColor?.ImageUrl,
						ExpectedQuantity = x.ExpectedQuantity // Số lượng FE hiển thị làm "mốc"
					}).ToList(),

				// 2. Danh sách kệ: FE lặp ra các dòng Checkbox
				ShelfItems = shipment.ShelfShipmentItems.Select(x => new ShipmentShelfItemDto
				{
					ShelfShipmentItemId = x.Id,
					ShelfId = x.ShelfId,
					ShelfCode = x.Shelf?.Code ?? "N/A",
					ShelfTypeName = x.Shelf?.ShelfType?.Name ?? "N/A"
				}).ToList()
			};
		}

		// Helper
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
				StoreReceivedAt = shipment.StoreReceivedAt,
				ReturnPickedUpAt = shipment.ReturnPickedUpAt,
				ArrivedWarehouseAt = shipment.ArrivedWarehouseAt,
				WarehouseReceivedAt = shipment.WarehouseReceivedAt
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
		private async Task HandleInventoryUpdateAsync(
			Guid fromLocId, Guid toLocId, Guid productColorId,
			InventoryStatus fromStatus, InventoryStatus toStatus,
			int qty, Guid referenceId, InventoryReferenceType refType)
		{
			if (qty <= 0) return;

			// 1. Cập nhật tồn kho (Tìm hoặc Tạo mới và Cộng dồn)
			var inv = await _inventoryRepository.GetAsync(toLocId, productColorId, toStatus);
			if (inv == null)
			{
				inv = new Inventory
				{
					Id = Guid.NewGuid(),
					InventoryLocationId = toLocId,
					ProductColorId = productColorId,
					Status = toStatus,
					Quantity = 0
				};
				await _inventoryRepository.AddAsync(inv);
			}
			inv.Quantity += qty;

			// 2. Ghi log Transaction tự động
			await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
			{
				Id = Guid.NewGuid(),
				ProductColorId = productColorId,
				FromLocationId = fromLocId,
				ToLocationId = toLocId,
				FromStatus = fromStatus,
				ToStatus = toStatus,
				Quantity = qty,
				ReferenceType = refType,
				ReferenceId = referenceId,
				CreatedAt = _dateTime.UtcNow
			});
		}
	}
}
