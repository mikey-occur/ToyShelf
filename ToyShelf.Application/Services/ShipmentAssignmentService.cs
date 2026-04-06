using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ShipmentAssignmentService : IShipmentAssignmentService
	{
		private readonly IShipmentAssignmentRepository _assignmentRepository;
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IUserWarehouseRepository _userWarehouseRepository;
		private readonly IShelfOrderRepository _repositoryShelfOrder;
		private readonly IDamageReportRepository _damageReportRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public ShipmentAssignmentService(
			IShipmentAssignmentRepository assignmentRepository,
			IStoreOrderRepository storeOrderRepository,
			IUserWarehouseRepository userWarehouseRepository,
			IShelfOrderRepository repositoryShelfOrder,
			IDamageReportRepository damageReportRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_assignmentRepository = assignmentRepository;
			_storeOrderRepository = storeOrderRepository;
			_userWarehouseRepository = userWarehouseRepository;
			_repositoryShelfOrder = repositoryShelfOrder;
			_damageReportRepository = damageReportRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// Thông minh : Cập nhật loại đơn
		private void UpdateAssignmentType(ShipmentAssignment assignment)
		{
			bool hasDelivery = assignment.StoreOrderId.HasValue || assignment.ShelfOrderId.HasValue;
			bool hasReturn = assignment.DamageReportId.HasValue;

			if (hasDelivery && hasReturn) assignment.Type = AssignmentType.Combined;
			else if (hasReturn) assignment.Type = AssignmentType.Return;
			else assignment.Type = AssignmentType.Delivery;
		}

		// ================= CREATE =================
		public async Task<ShipmentAssignmentResponse> CreateAsync(
			CreateShipmentAssignmentRequest request,
			ICurrentUser currentUser)
		{
			if (request.StoreOrderId == null && request.ShelfOrderId == null)
				throw new AppException("Order is required", 400);

			if (request.StoreOrderId != null && request.ShelfOrderId != null)
				throw new AppException("Only one order type is allowed", 400);

			Guid? storeLocationId = null;

			if (request.StoreOrderId != null)
			{
				var order = await _storeOrderRepository.GetByIdAsync(request.StoreOrderId.Value);
				if (order == null)
				{
					throw new AppException($"Store Order with ID {request.StoreOrderId.Value} not found.", 404);
				}

				if (order.Status != StoreOrderStatus.Approved)
				{
					throw new AppException($"Store Order is not approved (Current status: {order.Status}).", 400);
				}
				storeLocationId = order.StoreLocationId;
			}
			else if (request.ShelfOrderId != null)
			{
				var order = await _repositoryShelfOrder.GetByIdAsync(request.ShelfOrderId.Value);
				if (order == null)
				{
					throw new AppException($"Shelf Order with ID {request.ShelfOrderId.Value} not found.", 404);
				}

				if (order.Status != ShelfOrderStatus.Approved)
				{
					throw new AppException($"Shelf Order is not approved (Current status: {order.Status}).", 400);
				}
				storeLocationId = order.StoreLocationId;
			}

			// 2. LOGIC GÔM ĐƠN: Tìm xem Store này có Assignment nào đang PENDING (Ví dụ đang chờ thu hồi hàng hỏng)
			var existing = await _assignmentRepository.GetPendingByLocationAsync(
				request.WarehouseLocationId,
				storeLocationId!.Value);

			if (existing != null)
			{
				bool updated = false;

				// Nhồi StoreOrder nếu xe đang chờ và ngăn StoreOrder còn trống
				if (request.StoreOrderId != null && existing.StoreOrderId == null)
				{
					existing.StoreOrderId = request.StoreOrderId;
					updated = true;
				}
				// Nhồi ShelfOrder nếu xe đang chờ và ngăn ShelfOrder còn trống
				else if (request.ShelfOrderId != null && existing.ShelfOrderId == null)
				{
					existing.ShelfOrderId = request.ShelfOrderId;
					updated = true;
				}

				// Nếu nhồi thành công vào xe hiện có
				if (updated)
				{
					UpdateAssignmentType(existing); // Cập nhật lại Type (Delivery -> Combined nếu đã có Damage)
					_assignmentRepository.Update(existing);
					await _unitOfWork.SaveChangesAsync();

					var updatedResult = await _assignmentRepository.GetByIdWithDetailsAsync(existing.Id);
					return MapToResponse(updatedResult!);
				}
			}

			// 3. Nếu không gôm được, tạo mới
			var assignment = new ShipmentAssignment
			{
				Id = Guid.NewGuid(),
				StoreOrderId = request.StoreOrderId,
				ShelfOrderId = request.ShelfOrderId,
				WarehouseLocationId = request.WarehouseLocationId,
				Status = AssignmentStatus.Pending,
				CreatedAt = _dateTime.UtcNow,
				CreatedByUserId = currentUser.UserId
			};

			UpdateAssignmentType(assignment);

			await _assignmentRepository.AddAsync(assignment);
			await _unitOfWork.SaveChangesAsync();

			var result = await _assignmentRepository.GetByIdWithDetailsAsync(assignment.Id);

			if (result == null) throw new AppException("Shipment assignment not found", 404);

			return MapToResponse(result);
		}

		// TẠO TỪ DAMAGE REPORT
		public async Task CreateFromDamageReportAsync(Guid damageReportId, Guid warehouseLocationId, ICurrentUser currentUser)
		{
			var report = await _damageReportRepository.GetByIdAsync(damageReportId);
			if (report == null) throw new AppException("Report not found", 404);

			// Tìm xem có đơn giao hàng nào đang chờ đến Store đó không để đi nhờ xe
			var existing = await _assignmentRepository.GetPendingByLocationAsync(warehouseLocationId, report.InventoryLocationId);

			// CHỈ nhồi nếu tìm thấy xe và ngăn DamageReport của xe đó còn trống
			if (existing != null && existing.DamageReportId == null)
			{
				existing.DamageReportId = damageReportId;
				UpdateAssignmentType(existing);
				_assignmentRepository.Update(existing);
			}
			else
			{
				// Nếu không có xe nào đang chờ, hoặc xe đang chờ đã nhận 1 đơn thu hồi khác rồi
				var assignment = new ShipmentAssignment
				{
					Id = Guid.NewGuid(),
					DamageReportId = damageReportId,
					WarehouseLocationId = warehouseLocationId,
					Status = AssignmentStatus.Pending,
					Type = AssignmentType.Return,
					CreatedAt = _dateTime.UtcNow,
					CreatedByUserId = currentUser.UserId
				};
				UpdateAssignmentType(assignment);
				await _assignmentRepository.AddAsync(assignment);
			}
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= ASSIGN SHIPPER (WAREHOUSE) =================
		public async Task AssignShipperAsync(AssignShipperRequest request, ICurrentUser currentUser)
		{
			// 1. Lấy assignment 
			var assignment = await _assignmentRepository
				.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 2. Lấy WarehouseId thật (QUAN TRỌNG)
			var warehouseId = assignment.WarehouseLocation?.WarehouseId
				?? throw new AppException("Warehouse not found", 500);

			// 3. Check Manager 
			var userWarehouse = await _userWarehouseRepository
				.GetActiveAsync(currentUser.UserId, warehouseId);

			if (userWarehouse == null || userWarehouse.Role != WarehouseRole.Manager)
				throw new AppException("Only Manager can assign shipper", 403);

			//  4. Check shipper hợp lệ 
			var shipperWarehouse = await _userWarehouseRepository
				.GetActiveAsync(request.ShipperId, warehouseId);

			if (shipperWarehouse == null || shipperWarehouse.Role != WarehouseRole.Shipper)
				throw new AppException("User is not a shipper in this warehouse", 400);

			// 5. Check shipment đang chạy 
			var activeShipment = assignment.Shipments
				.FirstOrDefault(s => s.Status == ShipmentStatus.Draft
								  || s.Status == ShipmentStatus.Shipping);

			if (activeShipment != null)
				throw new AppException("Cannot change Shipper, shipment is in progress", 400);

			//  6. Assign 
			var oldShipperId = assignment.ShipperId;

			assignment.ShipperId = request.ShipperId;
			assignment.AssignedByUserId = currentUser.UserId;

			//  7. Update status 
			if (oldShipperId != request.ShipperId || assignment.Status == AssignmentStatus.Rejected)
			{
				assignment.Status = AssignmentStatus.Assigned;
				assignment.RespondedAt = null;
			}

			_assignmentRepository.Update(assignment);
			await _unitOfWork.SaveChangesAsync();
		}



		public async Task AcceptAsync(Guid id, ICurrentUser currentUser)
		{
			// 1. Lấy assignment 
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 2. Lấy WarehouseId chuẩn 
			var warehouseId = assignment.WarehouseLocation?.WarehouseId
				?? throw new AppException("Warehouse not found", 500);

			// 3. Check role Shipper 
			var userWarehouse = await _userWarehouseRepository
				.GetActiveAsync(currentUser.UserId, warehouseId);

			if (userWarehouse == null || userWarehouse.Role != WarehouseRole.Shipper)
				throw new AppException("Only Shipper can accept assignment", 403);

			// 4. Check đúng người được assign 
			if (assignment.ShipperId != currentUser.UserId)
				throw new AppException("You are not assigned to this shipment", 403);

			// 5. Check trạng thái 
			if (assignment.Status != AssignmentStatus.Assigned)
				throw new AppException("Assignment already responded", 400);

			// 6. Update 
			assignment.Status = AssignmentStatus.Accepted;
			assignment.RespondedAt = _dateTime.UtcNow;

			_assignmentRepository.Update(assignment);
			await _unitOfWork.SaveChangesAsync();
		}


		public async Task RejectAsync(Guid id, ICurrentUser currentUser)
		{
			// 1. Lấy assignment 
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 2. Lấy WarehouseId 
			var warehouseId = assignment.WarehouseLocation?.WarehouseId
				?? throw new AppException("Warehouse not found", 500);

			// 3. Check role Shipper 
			var userWarehouse = await _userWarehouseRepository
				.GetActiveAsync(currentUser.UserId, warehouseId);

			if (userWarehouse == null || userWarehouse.Role != WarehouseRole.Shipper)
				throw new AppException("Only Shipper can reject assignment", 403);

			// 4. Check đúng người
			if (assignment.ShipperId != currentUser.UserId)
				throw new AppException("You are not assigned to this shipment", 403);

			// 5. Check trạng thái
			if (assignment.Status != AssignmentStatus.Assigned)
				throw new AppException("Assignment already responded", 400);

			// 6. Update
			assignment.Status = AssignmentStatus.Rejected;
			assignment.RespondedAt = _dateTime.UtcNow;

			_assignmentRepository.Update(assignment);
			await _unitOfWork.SaveChangesAsync();
		}


		// ================= GET MY ASSIGNMENTS =================
		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetMyAssignments(ICurrentUser currentUser)
		{
			var assignments = await _assignmentRepository.GetByShipperIdWithOrderAsync(currentUser.UserId);

			return assignments.Select(MapToResponse);
		}

		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetByStoreOrderId(Guid storeOrderId)
		{
			var assignments = await _assignmentRepository
				.GetByStoreOrderIdWithDetailsAsync(storeOrderId);

			return assignments.Select(MapToResponse);
		}

		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetAllAsync()
		{
			var assignments = await _assignmentRepository.GetAllWithDetailsAsync();

			return assignments.Select(MapToResponse);
		}

		public async Task<ShipmentAssignmentResponse> GetByIdAsync(Guid id)
		{
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			return MapToResponse(assignment);
		}


		private static ShipmentAssignmentResponse MapToResponse(ShipmentAssignment assignment)
		{
			// Xác định các cờ trạng thái để biết Assignment này chứa những gì
			var hasStoreOrder = assignment.StoreOrderId != null;
			var hasShelfOrder = assignment.ShelfOrderId != null;
			var hasDamageReport = assignment.DamageReportId != null;

			// Lấy shipment mới nhất để xem trạng thái vận chuyển hiện tại
			var shipment = assignment.Shipments?
				.OrderByDescending(s => s.CreatedAt)
				.FirstOrDefault();

			// --- TẠO ORDER TYPE ĐỘNG ---
			var types = new List<string>();
			if (hasStoreOrder) types.Add("STORE");
			if (hasShelfOrder) types.Add("SHELF");
			if (hasDamageReport) types.Add("DAMAGE");

			// Kết quả sẽ ra dạng: "STORE", "STORE-SHELF", "STORE-SHELF-DAMAGE", v.v.
			string dynamicOrderType = types.Any() ? string.Join("-", types) : "UNKNOWN";

			var response = new ShipmentAssignmentResponse
			{
				Id = assignment.Id,

				// ===== THÔNG TIN ĐƠN HÀNG & BÁO CÁO =====
				StoreOrderId = assignment.StoreOrderId,
				StoreOrderCode = assignment.StoreOrder?.Code,

				ShelfOrderId = assignment.ShelfOrderId,
				ShelfOrderCode = assignment.ShelfOrder?.Code,

				DamageReportId = assignment.DamageReportId,
				DamageReportCode = assignment.DamageReport?.Code,

				// Phân loại đơn để UI hiển thị Label (STORE / SHELF / DAMAGE)
				OrderType = dynamicOrderType,

				// ===== THÔNG TIN ĐỊA ĐIỂM (LOCATION) =====
				WarehouseLocationId = assignment.WarehouseLocationId,
				WarehouseLocationName = assignment.WarehouseLocation?.Name ?? "",

				// Ưu tiên lấy StoreLocationId từ đơn giao, nếu không có thì lấy từ đơn thu hồi
				StoreLocationId = hasStoreOrder
					? (assignment.StoreOrder?.StoreLocationId ?? Guid.Empty)
					: (hasShelfOrder
						? (assignment.ShelfOrder?.StoreLocationId ?? Guid.Empty)
						: (assignment.DamageReport?.InventoryLocationId ?? Guid.Empty)),

				StoreLocationName = hasStoreOrder
					? (assignment.StoreOrder?.StoreLocation?.Name ?? "")
					: (hasShelfOrder
						? (assignment.ShelfOrder?.StoreLocation?.Name ?? "")
						: (assignment.DamageReport?.InventoryLocation?.Name ?? "")),

				// ===== THÔNG TIN NHÂN SỰ =====
				ShipperName = assignment.Shipper?.FullName,
				CreatedByName = assignment.CreatedByUser?.FullName
					?? throw new Exception("CreatedByUser not loaded"),
				AssignedByName = assignment.AssignedByUser?.FullName,

				// ===== TRẠNG THÁI & THỜI GIAN =====
				Type = assignment.Type, // Delivery / Return / Combined
				Status = assignment.Status,
				ShipmentStatus = shipment?.Status ?? ShipmentStatus.Draft,

				// Ghi chú thông minh cho Shipper
				AdminNote = hasDamageReport
					? $"[THU HỒI] {assignment.DamageReport?.AdminNote ?? "Vui lòng thu hồi hàng hỏng tại Store"}"
					: "Giao hàng theo đơn",

				CreatedAt = assignment.CreatedAt,
				RespondedAt = assignment.RespondedAt
			};

			// ================= 1. XỬ LÝ HÀNG GIAO (STORE ORDER) =================
			if (hasStoreOrder && assignment.StoreOrder != null)
			{
				response.ProductItems = assignment.StoreOrder.Items
					.Select(x => new ShipmentAssignmentProductItemResponse
					{
						ProductColorId = x.ProductColorId,
						SKU = x.ProductColor?.Product?.SKU ?? "N/A",
						ProductName = x.ProductColor?.Product?.Name ?? "Unknown",
						Color = x.ProductColor?.Color?.Name ?? "N/A",
						ImageUrl = x.ProductColor?.ImageUrl,
						Quantity = x.Quantity,
						FulfilledQuantity = x.FulfilledQuantity
					}).ToList();
			}

			// ================= 2. XỬ LÝ KỆ GIAO (SHELF ORDER) =================
			if (hasShelfOrder && assignment.ShelfOrder != null)
			{
				response.ShelfItems = assignment.ShelfOrder.Items
					.Select(x =>
					{
						var shelfType = x.ShelfType;
						return new ShipmentAssignmentShelfItemResponse
						{
							ShelfTypeId = x.ShelfTypeId,
							ShelfTypeName = x.ShelfTypeName,
							ImageUrl = x.ImageUrl,
							Width = shelfType?.Width ?? 0,
							Height = shelfType?.Height ?? 0,
							Depth = shelfType?.Depth ?? 0,
							TotalLevels = shelfType?.TotalLevels ?? 0,
							Quantity = x.Quantity,
							FulfilledQuantity = x.FulfilledQuantity,
						};
					}).ToList();
			}

			// ================= 3. XỬ LÝ HÀNG THU HỒI (DAMAGE REPORT) =================
			if (hasDamageReport && assignment.DamageReport != null)
			{
				var dr = assignment.DamageReport;

				// Tạo một object chi tiết món hàng cần thu hồi để Shipper đối soát
				response.DamageReturnItem = new ShipmentAssignmentDamageItemResponse
				{
					DamageReportId = dr.Id,
					DamageCode = dr.Code,
					DamageType = dr.Type.ToString(), // Product hoặc Shelf
					Source = dr.Source.ToString(),   // Lỗi do ai?
					Quantity = dr.Quantity,
					Description = dr.Description,

					// Thông tin định danh chi tiết
					TargetName = dr.Type == DamageType.Product
						? $"{dr.ProductColor?.Product?.Name} ({dr.ProductColor?.Color?.Name})"
						: $"Kệ: {dr.Shelf?.Code}",

					// Ưu tiên lấy ảnh sản phẩm, nếu không có lấy ảnh hiện trường Store đã chụp
					ImageUrl = dr.ProductColor?.ImageUrl ?? dr.DamageMedia.FirstOrDefault()?.MediaUrl
				};
			}

			return response;
		}
	}
}
