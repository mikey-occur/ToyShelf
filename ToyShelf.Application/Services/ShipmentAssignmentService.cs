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
			bool hasDelivery = assignment.AssignmentStoreOrders.Any() || assignment.AssignmentShelfOrders.Any();
			bool hasReturn = assignment.DamageReports.Any();

			if (hasDelivery && hasReturn) assignment.Type = AssignmentType.Combined;
			else if (hasReturn) assignment.Type = AssignmentType.Return;
			else assignment.Type = AssignmentType.Delivery;
		}

		// ================= CREATE (Từ Order) =================
		public async Task<ShipmentAssignmentResponse> CreateAsync(CreateShipmentAssignmentRequest request, ICurrentUser currentUser)
		{
			if (request.StoreOrderId == null && request.ShelfOrderId == null)
				throw new AppException("Ít nhất một đơn hàng (Hàng hoặc Kệ) là bắt buộc", 400);

			Guid storeLocationId;

			// Kiểm tra và lấy thông tin vị trí cửa hàng
			if (request.StoreOrderId != null)
			{
				var order = await _storeOrderRepository.GetByIdAsync(request.StoreOrderId.Value);
				if (order == null || order.Status != StoreOrderStatus.Approved)
					throw new AppException("Đơn hàng không tồn tại hoặc chưa được duyệt", 400);
				storeLocationId = order.StoreLocationId;
			}
			else
			{
				var order = await _repositoryShelfOrder.GetByIdAsync(request.ShelfOrderId!.Value);
				if (order == null || order.Status != ShelfOrderStatus.Approved)
					throw new AppException("Đơn kệ không tồn tại hoặc chưa được duyệt", 400);
				storeLocationId = order.StoreLocationId;
			}

			// TÌM XE ĐANG CHỜ (GOM ĐƠN)
			var assignment = await _assignmentRepository.GetPendingByLocationAsync(request.WarehouseLocationId, storeLocationId);

			if (assignment == null)
			{
				// TẠO MỚI NẾU CHƯA CÓ XE CHỜ
				assignment = new ShipmentAssignment
				{
					Id = Guid.NewGuid(),
					WarehouseLocationId = request.WarehouseLocationId,
					Status = AssignmentStatus.Pending,
					Type = AssignmentType.Delivery,
					CreatedAt = _dateTime.UtcNow,
					CreatedByUserId = currentUser.UserId
				};
				await _assignmentRepository.AddAsync(assignment);
			}

			// NHỒI ĐƠN VÀO BẢNG TRUNG GIAN (N-N)
			if (request.StoreOrderId.HasValue)
			{
				if (!assignment.AssignmentStoreOrders.Any(x => x.StoreOrderId == request.StoreOrderId))
				{
					assignment.AssignmentStoreOrders.Add(new AssignmentStoreOrder { StoreOrderId = request.StoreOrderId.Value });
				}
			}

			if (request.ShelfOrderId.HasValue)
			{
				if (!assignment.AssignmentShelfOrders.Any(x => x.ShelfOrderId == request.ShelfOrderId))
				{
					assignment.AssignmentShelfOrders.Add(new AssignmentShelfOrder { ShelfOrderId = request.ShelfOrderId.Value });
				}
			}

			UpdateAssignmentType(assignment);
			await _unitOfWork.SaveChangesAsync();

			var result = await _assignmentRepository.GetByIdWithDetailsAsync(assignment.Id);
			return MapToResponse(result!);
		}

		// Gôm đơn 1 - N
		public async Task CreateFromDamageReportAsync(Guid damageReportId, Guid warehouseLocationId, ICurrentUser currentUser)
		{
			// 1. Kiểm tra báo cáo hư hại
			var report = await _damageReportRepository.GetByIdAsync(damageReportId);
			if (report == null)
				throw new AppException("Damage report not found", 404);

			// 2. Tìm hoặc Khởi tạo Assignment (Xe)
			var assignment = await _assignmentRepository.GetPendingByLocationAsync(warehouseLocationId, report.InventoryLocationId);
			bool isNewAssignment = false;

			if (assignment == null)
			{
				assignment = new ShipmentAssignment
				{
					Id = Guid.NewGuid(),
					WarehouseLocationId = warehouseLocationId,
					Status = AssignmentStatus.Pending,
					Type = AssignmentType.Return, // Mặc định ban đầu là Return
					CreatedAt = _dateTime.UtcNow,
					CreatedByUserId = currentUser.UserId
				};
				isNewAssignment = true;
			}

			// 3. Thiết lập mối quan hệ (Link Report vào Xe)
			report.ShipmentAssignmentId = assignment.Id;

			// Cập nhật quan hệ trong bộ nhớ (In-memory) để logic UpdateAssignmentType chạy chính xác
			if (assignment.DamageReports == null) assignment.DamageReports = new List<DamageReport>();
			assignment.DamageReports.Add(report);

			// 4. Cập nhật trạng thái và Lưu dữ liệu
			UpdateAssignmentType(assignment);

			if (isNewAssignment)
			{
				await _assignmentRepository.AddAsync(assignment);
			}
			else
			{
				_assignmentRepository.Update(assignment);
			}

			_damageReportRepository.Update(report);

			// Lưu toàn bộ thay đổi trong 1 Transaction thông qua Unit of Work
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
			var storeOrders = assignment.AssignmentStoreOrders?.Select(x => x.StoreOrder).Where(x => x != null).ToList() ?? new();
			var shelfOrders = assignment.AssignmentShelfOrders?.Select(x => x.ShelfOrder).Where(x => x != null).ToList() ?? new();
			var damageReports = assignment.DamageReports?.ToList() ?? new();

			var shipment = assignment.Shipments?.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

			// Tạo Order Type động
			var types = new List<string>();
			if (storeOrders.Any()) types.Add("STORE");
			if (shelfOrders.Any()) types.Add("SHELF");
			if (damageReports.Any()) types.Add("DAMAGE");

			// Lấy địa điểm Store đầu tiên tìm thấy
			var firstStore = storeOrders.FirstOrDefault()?.StoreLocation ??
							 shelfOrders.FirstOrDefault()?.StoreLocation ??
							 damageReports.FirstOrDefault()?.InventoryLocation;

			var response = new ShipmentAssignmentResponse
			{
				Id = assignment.Id,
				OrderType = types.Any() ? string.Join("-", types) : "EMPTY",

				StoreOrderCodes = storeOrders.Select(o => o.Code).ToList(),
				ShelfOrderCodes = shelfOrders.Select(o => o.Code).ToList(),
				DamageReportCodes = damageReports.Select(dr => dr.Code).ToList(),

				WarehouseLocationId = assignment.WarehouseLocationId,
				WarehouseLocationName = assignment.WarehouseLocation?.Name ?? "",

				StoreLocationId = firstStore?.Id ?? Guid.Empty,
				StoreLocationName = firstStore?.Name ?? "N/A",

				ShipperName = assignment.Shipper?.FullName,
				CreatedByName = assignment.CreatedByUser?.FullName ?? "System",
				AssignedByName = assignment.AssignedByUser?.FullName,

				Type = assignment.Type,
				Status = assignment.Status,
				ShipmentStatus = shipment?.Status ?? ShipmentStatus.Draft,

				AdminNote = damageReports.Any()
					? $"[THU HỒI] Có {damageReports.Count} báo cáo cần lấy về kho."
					: "Giao hàng theo vận đơn của hệ thống.",

				CreatedAt = assignment.CreatedAt,
				RespondedAt = assignment.RespondedAt
			};

			// GOM TẤT CẢ HÀNG HÓA TỪ NHIỀU ĐƠN (SelectMany)
			response.ProductItems = storeOrders
				.SelectMany(o => o.Items)
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

			// GOM TẤT CẢ KỆ TỪ NHIỀU ĐƠN
			response.ShelfItems = shelfOrders
				.SelectMany(o => o.Items)
				.Select(x => new ShipmentAssignmentShelfItemResponse
				{
					ShelfTypeId = x.ShelfTypeId,
					ShelfTypeName = x.ShelfTypeName,
					ImageUrl = x.ImageUrl,
					Width = x.ShelfType?.Width ?? 0,
					Height = x.ShelfType?.Height ?? 0,
					Quantity = x.Quantity,
					FulfilledQuantity = x.FulfilledQuantity,
				}).ToList();

			// GOM THU HỒI
			response.DamageReturnItems = damageReports
				.SelectMany(dr => dr.Items.Select(item => new ShipmentAssignmentDamageItemResponse
				{
					DamageReportId = dr.Id,
					DamageCode = dr.Code,
					DamageType = item.DamageItemType.ToString(), // Product hoặc Shelf
					Source = dr.Source.ToString(),
					Description = dr.Description,

					// Logic hiển thị tên mục tiêu hỏng
					TargetName = item.DamageItemType == DamageItemType.Product
						? $"{item.ProductColor?.Product?.Name} ({item.ProductColor?.Color?.Name})"
						: $"Kệ: {item.Shelf?.Code}",

					Quantity = item.Quantity ?? 1,

					// Ưu tiên lấy ảnh bằng chứng hỏng hóc, nếu không có thì lấy ảnh Product mẫu
					ImageUrl = item.DamageMedia.FirstOrDefault()?.MediaUrl
							   ?? item.ProductColor?.ImageUrl
				})).ToList();

			return response;
		}
	}
}
