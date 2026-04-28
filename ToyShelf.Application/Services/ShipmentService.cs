using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.DamageReport.Response;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.Shipment.Response;
using ToyShelf.Application.Models.ShipmentAssignment.Response;
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
		public async Task<IEnumerable<ShipmentResponse>> GetByShelfOrderIdAsync(Guid shelfOrderId)
		{
			var shipments = await _shipmentRepository.GetByShelfOrderIdAsync(shelfOrderId);

			if (shipments == null || !shipments.Any())
				throw new AppException("No shipments found for this shelf order", 404);

			return shipments.Select(MapToResponse);
		}
		public async Task<IEnumerable<ShipmentResponse>> GetByDamageReportIdAsync(Guid damageReportId)
		{
			var shipments = await _shipmentRepository.GetByDamageReportIdAsync(damageReportId);

			if (shipments == null || !shipments.Any())
				throw new AppException("No shipments found for this damage report", 404);

			return shipments.Select(MapToResponse);
		}
		public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, ICurrentUser currentUser)
		{
			// 1. Validation & Load Data
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);
			if (assignment == null) throw new AppException("Assignment not found", 404);

			// Kiểm tra: Phải có ít nhất một thứ để vận chuyển
			// (Hoặc là hàng mới từ request, hoặc là hàng hỏng có sẵn trong assignment)

			var hasDamageToReturn = assignment.AssignmentDamageReports?.Any(x => x.DamageReport?.Items?.Any() ?? false) ?? false;
			var hasNewItemsInRequest = request.Products.Any() || request.Shelves.Any();

			if (!hasNewItemsInRequest && !hasDamageToReturn)
			{
				throw new AppException("Nhiệm vụ này không có hàng để giao và cũng không có hàng hỏng để thu hồi!", 400);
			}

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment before creating shipment", 400);

			if (assignment.ShipperId == null)
				throw new AppException("Shipper not assigned to this assignment", 400);

			// Xác định đích đến (ToLocation)
			var toLocationId = assignment.AssignmentStoreOrders.Select(x => (Guid?)x.StoreOrder.StoreLocationId).FirstOrDefault()
							?? assignment.AssignmentShelfOrders.Select(x => (Guid?)x.ShelfOrder.StoreLocationId).FirstOrDefault()
							?? assignment.AssignmentDamageReports?.Select(x => (Guid?)x.DamageReport.InventoryLocationId).FirstOrDefault();

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
				// Tìm AssignmentStoreOrder để xác thực đơn hàng này có nằm trong nhiệm vụ không
				var aso = assignment.AssignmentStoreOrders
					.FirstOrDefault(x => x.StoreOrderId == pReq.StoreOrderId);

				if (aso == null)
					throw new AppException($"Order {pReq.StoreOrderId} not in assignment", 400);

				var order = aso.StoreOrder; // Lấy ra đơn hàng từ bảng trung gian

				var orderItem = order.Items.FirstOrDefault(i => i.ProductColorId == pReq.ProductColorId);
				if (orderItem == null)
					throw new AppException("Product not found in store order", 400);

				// 1. Kiểm tra số lượng còn lại có thể giao (Remaining)
				var remaining = orderItem.Quantity - orderItem.FulfilledQuantity;
				if (pReq.ExpectedQuantity <= 0 || pReq.ExpectedQuantity > remaining)
					throw new AppException($"Invalid qty for {order.Code}. Max: {remaining}", 400);

				// 2. Kiểm tra tồn kho tại Warehouse
				var inventory = await _inventoryRepository.GetAsync(assignment.WarehouseLocationId, pReq.ProductColorId, InventoryStatus.Available);
				if (inventory == null || inventory.Quantity < pReq.ExpectedQuantity)
					throw new AppException($"Out of stock for {orderItem.ProductColor?.Product?.Name}", 400);

				// 3. Thêm vào ShipmentItem (Món hàng thực tế bốc lên xe)
				shipment.Items.Add(new ShipmentItem
				{
					Id = Guid.NewGuid(),
					StoreOrderItemId = orderItem.Id,
					ProductColorId = pReq.ProductColorId,
					ExpectedQuantity = pReq.ExpectedQuantity,
					ReceivedQuantity = 0
				});

				order.Status = StoreOrderStatus.Processing;
			}

			// 3. XỬ LÝ KỆ (Shelf Orders)
			foreach (var sReq in request.Shelves)
			{
				// TRUY VẤN LẠI: Lấy ShelfOrder thông qua bảng trung gian AssignmentShelfOrders
				var aso = assignment.AssignmentShelfOrders
					.FirstOrDefault(x => x.ShelfOrderId == sReq.ShelfOrderId);

				if (aso == null)
					throw new AppException($"Shelf Order {sReq.ShelfOrderId} not found in this assignment", 400);

				var sOrder = aso.ShelfOrder;

				var shelfOrderItem = sOrder.Items.FirstOrDefault(i => i.ShelfTypeId == sReq.ShelfTypeId);
				if (shelfOrderItem == null)
					throw new AppException("Shelf type not in this order", 400);

				// --- LOGIC DỊCH CHUYỂN TỒN KHO TRÊN SỔ SÁCH (InventoryShelf) ---
				var invAvailable = await _inventoryShelfRepository.GetShelfWithStatusAsync(
					assignment.WarehouseLocationId, sReq.ShelfTypeId, ShelfStatus.Available);

				if (invAvailable == null || !invAvailable.HasEnoughStock(sReq.ExpectedQuantity))
					throw new AppException($"Kho không đủ kệ sẵn dùng (Available). Loại: {shelfOrderItem.ShelfType?.Name}", 400);

				invAvailable.RemoveQuantity(sReq.ExpectedQuantity);

				var invReserved = await _inventoryShelfRepository.GetShelfWithStatusAsync(
					assignment.WarehouseLocationId, sReq.ShelfTypeId, ShelfStatus.Reserved);

				if (invReserved == null)
				{
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
					shelvesToReserve = await _shelfRepository.GetAvailableShelvesByType(
						assignment.WarehouseLocationId, sReq.ShelfTypeId, sReq.ExpectedQuantity);
				}

				if (shelvesToReserve.Count < sReq.ExpectedQuantity)
					throw new AppException($"Không đủ {sReq.ExpectedQuantity} kệ vật lý Available trong kho để gán định danh.", 400);

				// Cập nhật trạng thái từng cái tủ
				foreach (var shelf in shelvesToReserve)
				{
					shelf.Status = ShelfStatus.Reserved;

					shipment.ShelfShipmentItems.Add(new ShelfShipmentItem
					{
						Id = Guid.NewGuid(),
						ShelfId = shelf.Id,
						ShelfOrderItemId = shelfOrderItem.Id,
						Status = ShelfShipmentStatus.InTransit
					});
				}

				sOrder.Status = ShelfOrderStatus.Processing;
			}

			// 4. TỰ ĐỘNG XỬ LÝ THU HỒI (Damage Reports)
			var pendingDamageReports = assignment.AssignmentDamageReports?
				.Select(x => x.DamageReport)
				.Where(d => d != null && d.Status == DamageStatus.Approved)
				.ToList() ?? new List<DamageReport>();

			foreach (var report in pendingDamageReports)
			{
				// Cập nhật trạng thái báo cáo hỏng sang Scheduled (Đã lên lịch thu hồi)
				report.Status = DamageStatus.Scheduled;

				foreach (var dItem in report.Items ?? new List<DamageReportItem>())
				{
					shipment.Items.Add(new ShipmentItem
					{
						Id = Guid.NewGuid(),
						// Link vào Shipment hiện tại (EF sẽ tự map khi SaveChanges nếu Id là Identity)
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
			// Đảm bảo Repository đã Include ShelfShipmentItems và Shelf (để lấy ShelfTypeId)
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

				// 2. XỬ LÝ HÀNG MỚI (STORE ORDER) - Dịch chuyển Inventory (Sản phẩm)
				var storeOrderItems = shipment.Items.Where(x => x.StoreOrderItemId != null).ToList();
				foreach (var item in storeOrderItems)
				{
					var productColorId = item.ProductColorId!.Value;

					var inventoryAvailable = await _inventoryRepository.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.Available);
					if (inventoryAvailable == null || inventoryAvailable.Quantity < item.ExpectedQuantity)
						throw new AppException($"Not enough available stock for product {productColorId} at warehouse", 400);

					inventoryAvailable.Quantity -= item.ExpectedQuantity;

					var inventoryTransit = await _inventoryRepository.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.InTransit);
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
				// GroupBy theo ShelfTypeId để cập nhật InventoryShelf hiệu quả hơn
				var shelfItems = shipment.ShelfShipmentItems.ToList();

				// Lấy tất cả thông tin kệ vật lý để có ShelfTypeId
				var shelfGroups = shelfItems
					.Select(si => si.Shelf)
					.GroupBy(s => s.ShelfTypeId)
					.Select(g => new { ShelfTypeId = g.Key, Count = g.Count(), Shelves = g.ToList() });

				foreach (var group in shelfGroups)
				{
					// A. CẬP NHẬT INVENTORYSHELF 

					// 1. Trừ số lượng ở trạng thái Reserved tại kho đi
					var invReserved = await _inventoryShelfRepository.GetShelfWithStatusAsync(
						shipment.FromLocationId, group.ShelfTypeId, ShelfStatus.Reserved);

					if (invReserved == null || invReserved.Quantity < group.Count)
						throw new AppException("Dữ liệu tồn kho Reserved không đủ để thực hiện Pickup.", 400);

					invReserved.RemoveQuantity(group.Count);

					// 2. Tăng số lượng ở trạng thái InTransit tại kho đi
					var invTransit = await _inventoryShelfRepository.GetShelfWithStatusAsync(
						shipment.FromLocationId, group.ShelfTypeId, ShelfStatus.InTransit);

					if (invTransit == null)
					{
						invTransit = new InventoryShelf(shipment.FromLocationId, group.ShelfTypeId, group.Count, ShelfStatus.InTransit);
						await _inventoryShelfRepository.AddAsync(invTransit);
					}
					else
					{
						invTransit.AddQuantity(group.Count);
					}

					// B. CẬP NHẬT TỪNG KỆ VẬT LÝ (Định danh)
					foreach (var shelf in group.Shelves)
					{
						if (shelf.Status != ShelfStatus.Reserved)
							throw new AppException($"Shelf {shelf.Code} is not Reserved and cannot be picked up", 400);

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

				// 4. Update Header Shipment
				shipment.Status = ShipmentStatus.Shipping;
				shipment.PickedUpAt = _dateTime.UtcNow;

				_shipmentRepository.Update(shipment);

				// 5. Update Status Of Assignment
				var assignment = await _assignmentRepository
							.GetByIdAsync(shipment.ShipmentAssignmentId);

				if (assignment != null && assignment.Status != AssignmentStatus.InProgress)
				{
					assignment.Status = AssignmentStatus.InProgress;

					if (!assignment.InProgressAt.HasValue)
					{
						assignment.InProgressAt = _dateTime.UtcNow;
					}

					_assignmentRepository.Update(assignment);
				}

				// Lưu toàn bộ thay đổi (Inventory, InventoryShelf, Shelf, Transactions)
				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception)
			{
				throw;
			}
		}
		public async Task PickupReturnAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			// Đảm bảo GetByIdWithDetailsAsync đã Include ShipmentAssignment.AssignmentDamageReports.DamageReport.Items
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null) throw new AppException("Shipment not found", 404);

			// Chỉ cho phép thu hồi khi xe đang trong hành trình giao hoặc đã giao xong 1 phần
			if (shipment.Status != ShipmentStatus.Shipping && shipment.Status != ShipmentStatus.Delivered)
				throw new AppException("Invalid status to pickup return items", 400);

			// LẤY DANH SÁCH DAMAGE REPORTS QUA ASSIGNMENT
			var reports = shipment.ShipmentAssignment?.AssignmentDamageReports
				.Select(adr => adr.DamageReport)
				.Where(r => r != null && r.Status == DamageStatus.Scheduled)
				.ToList() ?? new List<DamageReport>();

			if (!reports.Any()) throw new AppException("No scheduled damage reports found for this shipment", 400);

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
					Purpose = ShipmentMediaPurpose.ReturnPickup,
					CreatedAt = _dateTime.UtcNow
				};
				await _shipmentMediaRepository.AddAsync(media);

				// 2. Duyệt qua các DamageReport thông qua danh sách đã lấy từ Assignment
				foreach (var report in reports)
				{
					// --- TRƯỜNG HỢP: SẢN PHẨM HỎNG ---
					var productItems = report.Items.Where(i => i.DamageItemType == DamageItemType.Product).ToList();
					foreach (var reportItem in productItems)
					{
						if (!reportItem.ProductColorId.HasValue) continue;

						var qty = reportItem.Quantity ?? 0;

						// Trừ kho Damaged tại Store
						var invDamaged = await _inventoryRepository.GetAsync(report.InventoryLocationId, reportItem.ProductColorId.Value, InventoryStatus.Damaged);
						if (invDamaged == null || invDamaged.Quantity < qty)
							throw new AppException($"Store does not have enough damaged stock for {reportItem.ProductColorId}", 400);

						invDamaged.Quantity -= qty;

						// Cộng kho InTransit (hàng đang trên xe về)
						var invTransit = await _inventoryRepository.GetAsync(report.InventoryLocationId, reportItem.ProductColorId.Value, InventoryStatus.InTransit);
						if (invTransit == null)
						{
							invTransit = new Inventory
							{
								Id = Guid.NewGuid(),
								InventoryLocationId = report.InventoryLocationId,
								ProductColorId = reportItem.ProductColorId.Value,
								Status = InventoryStatus.InTransit,
								Quantity = qty
							};
							await _inventoryRepository.AddAsync(invTransit);
						}
						else invTransit.Quantity += qty;

						// Log giao dịch
						await _inventoryTransactionRepository.AddAsync(new InventoryTransaction
						{
							Id = Guid.NewGuid(),
							ProductColorId = reportItem.ProductColorId.Value,
							FromLocationId = report.InventoryLocationId,
							ToLocationId = shipment.FromLocationId, // Về lại kho tổng
							FromStatus = InventoryStatus.Damaged,
							ToStatus = InventoryStatus.InTransit,
							Quantity = qty,
							ReferenceType = InventoryReferenceType.DamageReport,
							ReferenceId = report.Id,
							CreatedAt = _dateTime.UtcNow
						});
					}

					// --- TRƯỜNG HỢP: KỆ HỎNG ---
					var shelfItems = report.Items.Where(i => i.DamageItemType == DamageItemType.Shelf).ToList();
					var shelfGroups = shelfItems
						.Where(i => i.ShelfId.HasValue)
						.GroupBy(i => i.Shelf!.ShelfTypeId)
						.Select(g => new { ShelfTypeId = g.Key, Count = g.Count(), Items = g.ToList() });

					foreach (var group in shelfGroups)
					{
						// 1. Trừ số lượng Maintenance (Ngăn kệ hỏng tại Store)
						var invMaintenance = await _inventoryShelfRepository.GetShelfWithStatusAsync(
							report.InventoryLocationId, group.ShelfTypeId, ShelfStatus.Maintenance);

						if (invMaintenance == null || invMaintenance.Quantity < group.Count)
							throw new AppException($"Dữ liệu tồn kho bảo trì tại Store không đủ để bốc hàng.", 400);

						invMaintenance.RemoveQuantity(group.Count);

						// 2. Tăng số lượng InTransit
						var invTransit = await _inventoryShelfRepository.GetShelfWithStatusAsync(
							report.InventoryLocationId, group.ShelfTypeId, ShelfStatus.InTransit);

						if (invTransit == null)
						{
							invTransit = new InventoryShelf(report.InventoryLocationId, group.ShelfTypeId, group.Count, ShelfStatus.InTransit);
							await _inventoryShelfRepository.AddAsync(invTransit);
						}
						else invTransit.AddQuantity(group.Count);

						// Cập nhật từng tủ vật lý
						foreach (var item in group.Items)
						{
							var shelf = item.Shelf;
							if (shelf == null) continue;

							var oldStatus = shelf.Status;
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
					// Cập nhật trạng thái Report
					report.Status = DamageStatus.InTransit;
				}

				// 3. Update Header Shipment
				shipment.Status = ShipmentStatus.ShippingReturn;
				shipment.ReturnPickedUpAt = _dateTime.UtcNow;

				_shipmentRepository.Update(shipment);
				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception)
			{
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

				var assignment = shipment.ShipmentAssignment;

				if (assignment != null &&
					assignment.Shipments.All(s => s.Id == shipment.Id || s.Status == ShipmentStatus.Completed))
				{
					assignment.Status = AssignmentStatus.Completed;
					assignment.CompletedAt = _dateTime.UtcNow;
				}

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
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);
			if (shipment == null) throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Delivered)
				throw new AppException("Vận đơn chưa ở trạng thái Delivered để nhận hàng", 400);

			try
			{
				// ================= 1. XỬ LÝ SẢN PHẨM (Store Order) =================
				if (request.ProductItems != null)
				{
					foreach (var req in request.ProductItems)
					{
						var sItem = shipment.Items.FirstOrDefault(x => x.Id == req.ShipmentItemId);
						if (sItem == null || !sItem.ProductColorId.HasValue) continue;

						var productColorId = sItem.ProductColorId.Value;
						var actual = req.ReceivedQuantity;
						var expected = sItem.ExpectedQuantity;
						var missingOrDamaged = expected - actual;

						sItem.ReceivedQuantity = actual;

						if (sItem.StoreOrderItem != null)
						{
							sItem.StoreOrderItem.FulfilledQuantity += actual;
						}

						// TỒN KHO: A. Trừ hàng đang đi đường (InTransit) tại Warehouse nguồn
						var transit = await _inventoryRepository.GetAsync(shipment.FromLocationId, productColorId, InventoryStatus.InTransit);
						if (transit != null) transit.Quantity -= expected;

						// TỒN KHO: B. Cộng vào hàng sẵn sàng (Available) tại Store đích
						await HandleInventoryUpdateAsync(
							shipment.FromLocationId, shipment.ToLocationId, productColorId,
							InventoryStatus.InTransit, InventoryStatus.Available,
							actual, shipment.Id, InventoryReferenceType.Shipment);

						// TỒN KHO: C. Xử lý hỏng/mất (Nếu có)
						if (missingOrDamaged > 0)
						{
							await HandleInventoryUpdateAsync(
								shipment.FromLocationId, shipment.FromLocationId, productColorId,
								InventoryStatus.InTransit, InventoryStatus.Damaged,
								missingOrDamaged, shipment.Id, InventoryReferenceType.Shipment);
						}
					}
				}

				// ================= 2. XỬ LÝ KỆ (Shelf Order) =================
				if (request.ShelfItems != null)
				{
					// Group theo ShelfType để cập nhật InventoryShelf (Sổ cái) hiệu quả
					var shelfGroups = shipment.ShelfShipmentItems
						.Where(si => request.ShelfItems.Any(r => r.ShelfShipmentItemId == si.Id))
						.GroupBy(si => si.Shelf.ShelfTypeId)
						.Select(g => new
						{
							ShelfTypeId = g.Key,
							Items = g.ToList(),
							TotalExpected = g.Count()
						});

					foreach (var group in shelfGroups)
					{
						int actualReceivedCount = 0;
						int damagedCount = 0;

						foreach (var sItem in group.Items)
						{
							var reqShelf = request.ShelfItems.First(r => r.ShelfShipmentItemId == sItem.Id);

							if (reqShelf.IsReceived)
							{
								// A. Cập nhật định danh vật lý (Shelf)
								sItem.Shelf.InventoryLocationId = shipment.ToLocationId;
								sItem.Shelf.Status = ShelfStatus.InUse;
								sItem.Status = ShelfShipmentStatus.Received;

								if (sItem.ShelfOrderItem != null) sItem.ShelfOrderItem.FulfilledQuantity += 1;
								actualReceivedCount++;
							}
							else
							{
								// B. Xử lý kệ hỏng khi nhận
								sItem.Shelf.Status = ShelfStatus.Maintenance;
								sItem.Status = ShelfShipmentStatus.Damaged;
								damagedCount++;
							}
						}

						// C. CẬP NHẬT SỔ CÁI (InventoryShelf)

						// 1. Trừ InTransit tại Warehouse nguồn (Trừ toàn bộ số lượng đã bốc đi)
						var invTransit = await _inventoryShelfRepository.GetShelfWithStatusAsync(
							shipment.FromLocationId, group.ShelfTypeId, ShelfStatus.InTransit);
						if (invTransit != null) invTransit.RemoveQuantity(group.TotalExpected);

						// 2. Cộng InUse tại Store đích (Số lượng thực nhận)
						if (actualReceivedCount > 0)
						{
							var invInUse = await _inventoryShelfRepository.GetShelfWithStatusAsync(
								shipment.ToLocationId, group.ShelfTypeId, ShelfStatus.InUse);

							if (invInUse == null)
							{
								invInUse = new InventoryShelf(shipment.ToLocationId, group.ShelfTypeId, actualReceivedCount, ShelfStatus.InUse);
								await _inventoryShelfRepository.AddAsync(invInUse);
							}
							else invInUse.AddQuantity(actualReceivedCount);
						}

						// 3. Cộng Maintenance/Damaged tại Warehouse nguồn (Nếu kệ hỏng thì quay về Warehouse sửa)
						if (damagedCount > 0)
						{
							var invMaintenance = await _inventoryShelfRepository.GetShelfWithStatusAsync(
								shipment.FromLocationId, group.ShelfTypeId, ShelfStatus.Maintenance);

							if (invMaintenance == null)
							{
								invMaintenance = new InventoryShelf(shipment.FromLocationId, group.ShelfTypeId, damagedCount, ShelfStatus.Maintenance);
								await _inventoryShelfRepository.AddAsync(invMaintenance);
							}
							else invMaintenance.AddQuantity(damagedCount);
						}
					}
				}

				// ================= 3. CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG GỐC =================

				// Cập nhật ShelfOrders
				var relatedShelfOrderIds = shipment.ShelfShipmentItems
					.Select(x => x.ShelfOrderItem?.ShelfOrderId).Where(id => id.HasValue).Distinct();

				foreach (var shelfOrderId in relatedShelfOrderIds)
				{
					var sOrder = await _shelfOrderRepository.GetByIdAsync(shelfOrderId!.Value);
					if (sOrder != null)
					{
						bool isFull = sOrder.Items.All(i => i.FulfilledQuantity >= i.Quantity);
						sOrder.Status = isFull ? ShelfOrderStatus.Fulfilled : ShelfOrderStatus.PartiallyFulfilled;
					}
				}

				// Cập nhật StoreOrders
				var relatedOrderIds = shipment.Items
					.Select(x => x.StoreOrderItem?.StoreOrderId).Where(id => id.HasValue).Distinct();

				foreach (var orderId in relatedOrderIds)
				{
					var order = await _storeOrderRepository.GetByIdAsync(orderId!.Value);
					if (order != null)
					{
						bool isFull = order.Items.All(i => i.FulfilledQuantity >= i.Quantity);
						order.Status = isFull ? StoreOrderStatus.Fulfilled : StoreOrderStatus.PartiallyFulfilled;
					}
				}

				shipment.StoreReceivedAt = _dateTime.UtcNow;

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

			// Trạng thái DeliveredReturn nghĩa là Shipper đã về đến kho và bấm xác nhận trả hàng
			if (shipment.Status != ShipmentStatus.DeliveredReturn)
				throw new AppException("Vận đơn chưa ở trạng thái chờ nhận hàng trả (DeliveredReturn)", 400);

			// TRUY XUẤT DAMAGE REPORTS QUA ASSIGNMENT
			var damageReports = shipment.ShipmentAssignment?.AssignmentDamageReports
				.Select(adr => adr.DamageReport)
				.Where(r => r != null && r.Status == DamageStatus.InTransit) // Chỉ xử lý những report đang đi đường
				.ToList() ?? new List<DamageReport>();

			try
			{
				// ================= 1. XỬ LÝ THU HỒI HÀNG HÓA & KỆ (DAMAGE REPORTS) =================
				foreach (var report in damageReports)
				{
					// --- A. XỬ LÝ SẢN PHẨM HỎNG ---
					var productItems = report.Items.Where(i => i.DamageItemType == DamageItemType.Product).ToList();
					foreach (var rItem in productItems)
					{
						if (!rItem.ProductColorId.HasValue) continue;

						var productColorId = rItem.ProductColorId.Value;
						var qty = rItem.Quantity ?? 0;

						// 1. Trừ InTransit tại địa điểm Store (nơi shipper đã bốc lên)
						var transit = await _inventoryRepository.GetAsync(report.InventoryLocationId, productColorId, InventoryStatus.InTransit);
						if (transit != null) transit.Quantity -= qty;

						// 2. Cộng vào kho Damaged tại Warehouse (Xác nhận hàng đã về kho an toàn)
						// Sử dụng shipment.FromLocationId (thường là WarehouseId gốc của chuyến đi)
						await HandleInventoryUpdateAsync(
							report.InventoryLocationId, shipment.FromLocationId, productColorId,
							InventoryStatus.InTransit, InventoryStatus.Damaged,
							qty, report.Id, InventoryReferenceType.DamageReport);
					}

					// --- B. XỬ LÝ KỆ HỎNG ---
					var shelfItems = report.Items.Where(i => i.DamageItemType == DamageItemType.Shelf).ToList();
					var shelfGroups = shelfItems
						.Where(i => i.ShelfId.HasValue)
						.GroupBy(i => i.Shelf!.ShelfTypeId)
						.Select(g => new { ShelfTypeId = g.Key, Count = g.Count(), Items = g.ToList() });

					foreach (var group in shelfGroups)
					{
						// 1. Cập nhật Sổ cái InventoryShelf

						// Trừ InTransit tại Store nguồn (InventoryLocationId trong report)
						var invTransit = await _inventoryShelfRepository.GetShelfWithStatusAsync(
							report.InventoryLocationId, group.ShelfTypeId, ShelfStatus.InTransit);
						if (invTransit != null) invTransit.RemoveQuantity(group.Count);

						// Cộng Maintenance tại Warehouse đích (FromLocationId của shipment)
						var invWarehouseMaintenance = await _inventoryShelfRepository.GetShelfWithStatusAsync(
							shipment.FromLocationId, group.ShelfTypeId, ShelfStatus.Maintenance);

						if (invWarehouseMaintenance == null)
						{
							invWarehouseMaintenance = new InventoryShelf(shipment.FromLocationId, group.ShelfTypeId, group.Count, ShelfStatus.Maintenance);
							await _inventoryShelfRepository.AddAsync(invWarehouseMaintenance);
						}
						else invWarehouseMaintenance.AddQuantity(group.Count);

						// 2. Cập nhật từng thực thể Shelf vật lý
						foreach (var item in group.Items)
						{
							var shelf = item.Shelf;
							if (shelf == null) continue;

							// Di chuyển vị trí vật lý về Warehouse và đổi trạng thái
							shelf.InventoryLocationId = shipment.FromLocationId;
							shelf.Status = ShelfStatus.Maintenance;

							await _shelfTransactionRepository.AddAsync(new ShelfTransaction
							{
								Id = Guid.NewGuid(),
								ShelfId = shelf.Id,
								FromLocationId = report.InventoryLocationId,
								ToLocationId = shipment.FromLocationId,
								FromStatus = ShelfStatus.InTransit,
								ToStatus = ShelfStatus.Maintenance,
								ReferenceType = ShelfReferenceType.DamageReport,
								ReferenceId = report.Id,
								CreatedAt = _dateTime.UtcNow
							});
						}
					}

					report.Status = DamageStatus.Returned; // Kết thúc vòng đời Damage Report
				}

				// ================= 2. KẾT THÚC VÒNG ĐỜI VẬN ĐƠN =================
				shipment.Status = ShipmentStatus.Completed;
				shipment.WarehouseReceivedAt = _dateTime.UtcNow;

				_shipmentRepository.Update(shipment);

				var assignment = shipment.ShipmentAssignment;

				if (assignment != null &&
					assignment.Status != AssignmentStatus.Completed &&
					assignment.Shipments.All(s => s.Id == shipment.Id || s.Status == ShipmentStatus.Completed))
				{
					assignment.Status = AssignmentStatus.Completed;
					assignment.CompletedAt = _dateTime.UtcNow;
				}

				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new AppException($"Warehouse return receiving failed: {ex.Message}", 500);
			}
		}

		// Hàm kiểm tra Shipment -> Completed, để update ShipmentAssignment tương ứng nếu có
		//private async Task TryCompleteAssignmentAsync(Guid assignmentId)
		//{
		//	var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(assignmentId);
		//	if (assignment == null) return;

		//	// Check tất cả shipment đã Completed chưa
		//	var allCompleted = assignment.Shipments
		//		.All(s => s.Status == ShipmentStatus.Completed);

		//	if (!allCompleted) return;

		//	assignment.Status = AssignmentStatus.Completed;
		//	assignment.CompletedAt = _dateTime.UtcNow;

		//	_assignmentRepository.Update(assignment);
		//}
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
					ShelfTypeName = x.Shelf?.ShelfType?.Name ?? "N/A",
				    ImageUrl = x.Shelf?.ShelfType?.ImageUrl,
					Width = x.Shelf?.ShelfType?.Width ?? 0,
					Height = x.Shelf?.ShelfType?.Height ?? 0,
					Depth = x.Shelf?.ShelfType?.Depth ?? 0,
					TotalLevels = x.Shelf?.ShelfType?.TotalLevels ?? 0
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
			var storeOrdersFromAssignment = shipment.ShipmentAssignment?.AssignmentStoreOrders
										.Select(aso => aso.StoreOrder)
										.Where(o => o != null)
										.ToList() ?? new List<StoreOrder>();

			var shelfOrdersFromAssignment = shipment.ShipmentAssignment?.AssignmentShelfOrders
										.Select(aso => aso.ShelfOrder)
										.Where(o => o != null)
										.ToList() ?? new List<ShelfOrder>();

			var damageReports = shipment.ShipmentAssignment?.AssignmentDamageReports
										.Select(adr => adr.DamageReport)
										.Where(dr => dr != null)
										.ToList() ?? new List<DamageReport>();

			// 1. Xác định loại vận đơn thông minh hơn
			string orderType = shipment.IsReturn ? "RETURN" : "STORE";
			if (!shipment.IsReturn)
			{
				bool hasStore = storeOrdersFromAssignment.Any();
				bool hasShelf = shelfOrdersFromAssignment.Any();

				if (hasStore && hasShelf) orderType = "MIXED";
				else if (hasShelf) orderType = "SHELF";
				else if (hasStore) orderType = "STORE";
			}

			var response = new ShipmentResponse
			{
				Id = shipment.Id,
				Code = shipment.Code,
				IsReturn = shipment.IsReturn,
				OrderType = orderType,

				// Trả về danh sách IDs để FE có thể link tới chi tiết từng đơn con
				StoreOrders = storeOrdersFromAssignment
					.Select(x => new OrderReferenceResponse
					{
						Id = x.Id,
						Code = x.Code
					}).ToList(),

				ShelfOrders = shelfOrdersFromAssignment
					.Select(x => new OrderReferenceResponse
					{
						Id = x.Id,
						Code = x.Code
					}).ToList(),

				DamageReports = damageReports
					.Select(x => new OrderReferenceResponse
					{
						Id = x.Id,
						Code = x.Code
					}).ToList(),


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

			var deliveryItems = shipment.Items
				.Where(x => x.StoreOrderItemId.HasValue)
				.ToList();

			if (deliveryItems.Any())
			{
				response.ProductItems = deliveryItems.Select(x => new ShipmentProductItemResponse
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

			// ================= 2. KỆ ĐI GIAO (SHELF ORDER) =================
			if (shipment.ShelfShipmentItems != null && shipment.ShelfShipmentItems.Any())
			{
				response.ShelfItems = shipment.ShelfShipmentItems
					.Where(x => x.Shelf != null)
					.GroupBy(x => x.Shelf!.ShelfTypeId)
					.Select(g => {
						var shelfType = g.First().Shelf?.ShelfType;
						return new ShipmentShelfItemResponse
						{
							ShelfTypeId = g.Key,
							ShelfTypeName = shelfType?.Name ?? "Unknown",
							ImageUrl = shelfType?.ImageUrl,
							Width = shelfType?.Width ?? 0,
							Height = shelfType?.Height ?? 0,
							Depth = shelfType?.Depth ?? 0,
							TotalLevels = shelfType?.TotalLevels ?? 0,
							ExpectedQuantity = g.Count(),
							ReceivedQuantity = g.Count(x => x.Status == ShelfShipmentStatus.Received)
						};
					}).ToList();
			}

			// ================= 3. HÀNG THU HỒI (DAMAGE REPORT) =================
			var returnItems = shipment.Items
				.Where(x => x.DamageReportItemId.HasValue)
				.ToList();

			if (returnItems.Any())
			{
				response.DamageReturnItems = returnItems.Select(x => new DamageReturnItemResponse
				{
					DamageReportItemId = x.DamageReportItemId!.Value,
					DamageReportId = x.DamageReportItem?.DamageReportId ?? Guid.Empty,
					DamageCode = x.DamageReportItem?.DamageReport?.Code ?? "N/A",
					// Phân loại nhanh: Sản phẩm hay Kệ
					DamageType = x.ShelfId.HasValue ? "Shelf" : "Product",
					Quantity = x.ExpectedQuantity,
					// Nếu là kệ hiện mã kệ SH-..., nếu là SP hiện tên SP
					TargetName = x.ShelfId.HasValue
								 ? $"Kệ: {x.Shelf?.Code}"
								 : x.ProductColor?.Product?.Name ?? "Unknown",
					ImageUrl = x.ProductColor?.ImageUrl ?? x.Shelf?.ShelfType?.ImageUrl
				}).ToList();
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
