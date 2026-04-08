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
			// KIỂM TRA TRONG DANH SÁCH 1-N
			bool hasReturn = assignment.DamageReports.Any();

			if (hasDelivery && hasReturn) assignment.Type = AssignmentType.Combined;
			else if (hasReturn) assignment.Type = AssignmentType.Return;
			else assignment.Type = AssignmentType.Delivery;
		}

		// ================= CREATE (Từ Order) =================
		public async Task<ShipmentAssignmentResponse> CreateAsync(CreateShipmentAssignmentRequest request, ICurrentUser currentUser)
		{
			if (request.StoreOrderId == null && request.ShelfOrderId == null)
				throw new AppException("Order is required", 400);

			Guid? storeLocationId = null;

			if (request.StoreOrderId != null)
			{
				var order = await _storeOrderRepository.GetByIdAsync(request.StoreOrderId.Value);
				if (order == null || order.Status != StoreOrderStatus.Approved)
					throw new AppException("Order not found or not approved", 400);
				storeLocationId = order.StoreLocationId;
			}
			else if (request.ShelfOrderId != null)
			{
				var order = await _repositoryShelfOrder.GetByIdAsync(request.ShelfOrderId.Value);
				if (order == null || order.Status != ShelfOrderStatus.Approved)
					throw new AppException("Order not found or not approved", 400);
				storeLocationId = order.StoreLocationId;
			}

			// Gôm đơn vào Assignment hiện có
			var existing = await _assignmentRepository.GetPendingByLocationAsync(request.WarehouseLocationId, storeLocationId!.Value);

			if (existing != null)
			{
				bool updated = false;
				if (request.StoreOrderId != null && existing.StoreOrderId == null)
				{
					existing.StoreOrderId = request.StoreOrderId;
					updated = true;
				}
				else if (request.ShelfOrderId != null && existing.ShelfOrderId == null)
				{
					existing.ShelfOrderId = request.ShelfOrderId;
					updated = true;
				}

				if (updated)
				{
					UpdateAssignmentType(existing);
					_assignmentRepository.Update(existing);
					await _unitOfWork.SaveChangesAsync();
					var updatedResult = await _assignmentRepository.GetByIdWithDetailsAsync(existing.Id);
					return MapToResponse(updatedResult!);
				}
			}

			// Tạo mới nếu không gôm được
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
			return MapToResponse(result!);
		}

		// ================= CREATE TỪ DAMAGE REPORT (Gôm đơn 1-N) =================
		public async Task CreateFromDamageReportAsync(Guid damageReportId, Guid warehouseLocationId, ICurrentUser currentUser)
		{
			var report = await _damageReportRepository.GetByIdAsync(damageReportId);
			if (report == null) throw new AppException("Report not found", 404);

			// Tìm xe đang PENDING đi từ Kho này đến Store của report
			var existing = await _assignmentRepository.GetPendingByLocationAsync(warehouseLocationId, report.InventoryLocationId);

			if (existing != null)
			{
				// VÌ LÀ 1-N: Cứ nhồi vào danh sách, không cần check null nữa
				report.ShipmentAssignmentId = existing.Id;
				_damageReportRepository.Update(report);

				UpdateAssignmentType(existing);
				_assignmentRepository.Update(existing);
			}
			else
			{
				var assignment = new ShipmentAssignment
				{
					Id = Guid.NewGuid(),
					WarehouseLocationId = warehouseLocationId,
					Status = AssignmentStatus.Pending,
					Type = AssignmentType.Return,
					CreatedAt = _dateTime.UtcNow,
					CreatedByUserId = currentUser.UserId
				};
				await _assignmentRepository.AddAsync(assignment);

				report.ShipmentAssignmentId = assignment.Id;
				_damageReportRepository.Update(report);
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
			// 1. Xác định các cờ trạng thái dựa trên dữ liệu thực tế
			var hasStoreOrder = assignment.StoreOrderId != null;
			var hasShelfOrder = assignment.ShelfOrderId != null;
			// Kiểm tra danh sách DamageReports thay vì 1 ID đơn lẻ
			var hasDamageReports = assignment.DamageReports != null && assignment.DamageReports.Any();

			// 2. Lấy shipment mới nhất để theo dõi trạng thái vận chuyển
			var shipment = assignment.Shipments?
				.OrderByDescending(s => s.CreatedAt)
				.FirstOrDefault();

			// 3. Tạo Order Type động (Ví dụ: "STORE-DAMAGE" hoặc "SHELF-DAMAGE")
			var types = new List<string>();
			if (hasStoreOrder) types.Add("STORE");
			if (hasShelfOrder) types.Add("SHELF");
			if (hasDamageReports) types.Add("DAMAGE");

			string dynamicOrderType = types.Any() ? string.Join("-", types) : "UNKNOWN";

			// 4. Khởi tạo Response cơ bản
			var response = new ShipmentAssignmentResponse
			{
				Id = assignment.Id,

				StoreOrderId = assignment.StoreOrderId,
				StoreOrderCode = assignment.StoreOrder?.Code,

				ShelfOrderId = assignment.ShelfOrderId,
				ShelfOrderCode = assignment.ShelfOrder?.Code,

				OrderType = dynamicOrderType,

				WarehouseLocationId = assignment.WarehouseLocationId,
				WarehouseLocationName = assignment.WarehouseLocation?.Name ?? "",

				// Logic lấy Store Location thông minh: Ưu tiên đơn Giao -> đơn Kệ -> đơn Thu hồi
				StoreLocationId = hasStoreOrder
					? (assignment.StoreOrder?.StoreLocationId ?? Guid.Empty)
					: (hasShelfOrder
						? (assignment.ShelfOrder?.StoreLocationId ?? Guid.Empty)
						: (assignment.DamageReports?.FirstOrDefault()?.InventoryLocationId ?? Guid.Empty)),

				StoreLocationName = hasStoreOrder
					? (assignment.StoreOrder?.StoreLocation?.Name ?? "")
					: (hasShelfOrder
						? (assignment.ShelfOrder?.StoreLocation?.Name ?? "")
						: (assignment.DamageReports?.FirstOrDefault()?.InventoryLocation?.Name ?? "")),

				ShipperName = assignment.Shipper?.FullName,
				CreatedByName = assignment.CreatedByUser?.FullName ?? "System",
				AssignedByName = assignment.AssignedByUser?.FullName,

				Type = assignment.Type,
				Status = assignment.Status,
				ShipmentStatus = shipment?.Status ?? ShipmentStatus.Draft,

				AdminNote = hasDamageReports
					? $"[THU HỒI] Có {assignment.DamageReports?.Count ?? 0} báo cáo món hàng cần lấy về kho."
					: "Giao hàng theo vận đơn của hệ thống.",

				CreatedAt = assignment.CreatedAt,
				RespondedAt = assignment.RespondedAt
			};

			// StoreOrder 
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

			// ShelfOrder
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

			// DamageReport
			if (hasDamageReports && assignment.DamageReports != null)
			{
				response.DamageReturnItems = assignment.DamageReports.Select(dr => new ShipmentAssignmentDamageItemResponse
				{
					DamageReportId = dr.Id,
					DamageCode = dr.Code,
					DamageType = dr.Type.ToString(),
					Source = dr.Source.ToString(),
					Quantity = dr.Quantity,
					Description = dr.Description,

					TargetName = dr.Type == DamageType.Product
						? $"{dr.ProductColor?.Product?.Name} ({dr.ProductColor?.Color?.Name})"
						: $"Kệ: {dr.Shelf?.Code}",

					ImageUrl = dr.ProductColor?.ImageUrl ?? dr.DamageMedia.FirstOrDefault()?.MediaUrl
				}).ToList();
			}

			return response;
		}
	}
}
