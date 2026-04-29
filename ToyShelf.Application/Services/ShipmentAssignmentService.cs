using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Notification.Request;
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
		private readonly IAssignmentStoreOrderRepository _assignmentStoreOrderRepository;
		private readonly IAssignmentShelfOrderRepository _assignmentShelfOrderRepository;
		private readonly IUserRepository _userRepository;
		private readonly INotificationService _notificationService;
		private readonly ILogger<ShipmentAssignmentService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public ShipmentAssignmentService(
			IShipmentAssignmentRepository assignmentRepository,
			IStoreOrderRepository storeOrderRepository,
			IUserWarehouseRepository userWarehouseRepository,
			IShelfOrderRepository repositoryShelfOrder,
			IDamageReportRepository damageReportRepository,
			IAssignmentStoreOrderRepository assignmentStoreOrderRepository,
			IAssignmentShelfOrderRepository assignmentShelfOrderRepository,
			IUserRepository userRepository,
			INotificationService notificationService,
			ILogger<ShipmentAssignmentService> logger,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_assignmentRepository = assignmentRepository;
			_storeOrderRepository = storeOrderRepository;
			_userWarehouseRepository = userWarehouseRepository;
			_repositoryShelfOrder = repositoryShelfOrder;
			_damageReportRepository = damageReportRepository;
			_assignmentStoreOrderRepository = assignmentStoreOrderRepository;
			_assignmentShelfOrderRepository = assignmentShelfOrderRepository;
			_userRepository = userRepository;
			_notificationService = notificationService;
			_logger = logger;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		private async Task NotifyUsersAsync(
			List<User> users,
			string title,
			string content,
			string refType,
			Guid refId)
		{
			var tasks = users.Select(async user =>
			{
				try
				{
					await _notificationService.CreateInternalNotificationAsync(
						new InternalCreateNotificationRequest
						{
							UserId = user.Id,
							Title = title,
							Content = content,
							RefType = refType,
							RefId = refId
						}
					);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Gửi thông báo thất bại cho user {user.Id}");
				}
			});

			await Task.WhenAll(tasks);
		}

		// Thông minh : Cập nhật loại đơn
		private void UpdateAssignmentType(ShipmentAssignment assignment)
		{
			bool hasDelivery = assignment.AssignmentStoreOrders.Any() || assignment.AssignmentShelfOrders.Any();
			bool hasReturn = assignment.AssignmentDamageReports.Any();

			if (hasDelivery && hasReturn) assignment.Type = AssignmentType.Combined;
			else if (hasReturn) assignment.Type = AssignmentType.Return;
			else assignment.Type = AssignmentType.Delivery;
		}

		public async Task<ShipmentAssignmentResponse> CreateAsync(CreateShipmentAssignmentRequest request, ICurrentUser currentUser)
		{
			await _unitOfWork.BeginTransactionAsync();

			try
			{
				if (!request.StoreOrderId.HasValue && !request.ShelfOrderId.HasValue)
					throw new AppException("Ít nhất một đơn hàng (Hàng hoặc Kệ) là bắt buộc", 400);

				Guid storeLocationId;

				// Xác định location
				if (request.StoreOrderId.HasValue)
				{
					var storeOrder = await _storeOrderRepository.GetByIdAsync(request.StoreOrderId.Value);
					if (storeOrder == null || storeOrder.Status != StoreOrderStatus.Approved)
						throw new AppException("Đơn hàng Store không hợp lệ", 400);

					storeLocationId = storeOrder.StoreLocationId;
				}
				else
				{
					var shelfOrder = await _repositoryShelfOrder.GetByIdAsync(request.ShelfOrderId!.Value);
					if (shelfOrder == null || shelfOrder.Status != ShelfOrderStatus.Approved)
						throw new AppException("Đơn hàng Shelf không hợp lệ", 400);

					storeLocationId = shelfOrder.StoreLocationId;
				}

				var assignment = await _assignmentRepository
					.GetPendingByLocationAsync(request.WarehouseLocationId, storeLocationId);

				if (assignment == null)
				{
					assignment = await _assignmentRepository
						.GetPendingByLocationAsync(request.WarehouseLocationId, storeLocationId);

					if (assignment == null)
					{
						assignment = new ShipmentAssignment
						{
							Id = Guid.NewGuid(),
							WarehouseLocationId = request.WarehouseLocationId,
							Status = AssignmentStatus.Pending,
							Type = AssignmentType.Delivery,
							CreatedAt = _dateTime.UtcNow,
							CreatedByUserId = currentUser.UserId,
							AssignmentStoreOrders = new List<AssignmentStoreOrder>(),
							AssignmentShelfOrders = new List<AssignmentShelfOrder>()
						};

						await _assignmentRepository.AddAsync(assignment);
					}
				}

				// ===== STORE ORDER =====
				if (request.StoreOrderId.HasValue && request.StoreItems?.Any() == true)
				{
					var order = await _storeOrderRepository.GetByIdWithItemsAsync(request.StoreOrderId.Value);
					if (order == null) throw new AppException("Không tìm thấy StoreOrder", 404);

					var aso = await _unitOfWork.Repository<AssignmentStoreOrder>()
						.GetAsync(x =>
							x.ShipmentAssignmentId == assignment.Id &&
							x.StoreOrderId == request.StoreOrderId.Value);

					if (aso == null)
					{
						aso = new AssignmentStoreOrder
						{
							Id = Guid.NewGuid(),
							StoreOrderId = request.StoreOrderId.Value,
							ShipmentAssignmentId = assignment.Id
						};

						await _assignmentStoreOrderRepository.AddAsync(aso);
					}

					foreach (var itemReq in request.StoreItems)
					{
						var originalItem = order.Items.FirstOrDefault(x => x.Id == itemReq.ItemId);
						if (originalItem == null)
							throw new AppException($"Item không tồn tại", 400);

						int alreadyAllocated = await _assignmentRepository
							.GetTotalAllocatedQuantityAsync(request.StoreOrderId.Value, itemReq.ItemId);

						if (alreadyAllocated + itemReq.Quantity > originalItem.Quantity)
							throw new AppException("Vượt số lượng", 400);

						await _unitOfWork.Repository<AssignmentStoreOrderItem>().AddAsync(new AssignmentStoreOrderItem
						{
							Id = Guid.NewGuid(),
							AssignmentStoreOrderId = aso.Id,
							StoreOrderItemId = itemReq.ItemId,
							AllocatedQuantity = itemReq.Quantity
						});
					}
				}

				// ===== SHELF ORDER =====
				if (request.ShelfOrderId.HasValue && request.ShelfItems?.Any() == true)
				{
					var shelfOrder = await _repositoryShelfOrder.GetByIdWithItemsAsync(request.ShelfOrderId.Value);
					if (shelfOrder == null) throw new AppException("Không tìm thấy ShelfOrder", 404);

					var asho = await _unitOfWork.Repository<AssignmentShelfOrder>()
						.GetAsync(x =>
							x.ShipmentAssignmentId == assignment.Id &&
							x.ShelfOrderId == request.ShelfOrderId.Value);

					if (asho == null)
					{
						asho = new AssignmentShelfOrder
						{
							Id = Guid.NewGuid(),
							ShelfOrderId = request.ShelfOrderId.Value,
							ShipmentAssignmentId = assignment.Id
						};

						await _assignmentShelfOrderRepository.AddAsync(asho);
					}

					foreach (var itemReq in request.ShelfItems)
					{
						var originalItem = shelfOrder.Items.FirstOrDefault(x => x.Id == itemReq.ItemId);
						if (originalItem == null)
							throw new AppException("Item không tồn tại", 400);

						int alreadyAllocated = await _assignmentRepository
							.GetTotalShelfAllocatedQuantityAsync(request.ShelfOrderId.Value, itemReq.ItemId);

						if (alreadyAllocated + itemReq.Quantity > originalItem.Quantity)
							throw new AppException("Vượt số lượng", 400);

						await _unitOfWork.Repository<AssignmentShelfOrderItem>().AddAsync(new AssignmentShelfOrderItem
						{
							Id = Guid.NewGuid(),
							AssignmentShelfOrderId = asho.Id,
							ShelfOrderItemId = itemReq.ItemId,
							AllocatedQuantity = itemReq.Quantity
						});
					}
				}

				UpdateAssignmentType(assignment);

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				var result = await _assignmentRepository.GetByIdWithDetailsAsync(assignment.Id);
				return MapToResponse(result!);
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		// Gôm đơn 1 - N
		public async Task CreateFromDamageReportAsync(Guid damageReportId, Guid warehouseLocationId, ICurrentUser currentUser)
		{
			var report = await _damageReportRepository.GetByIdAsync(damageReportId);
			if (report == null) throw new AppException("Damage report not found", 404);

			var assignment = await _assignmentRepository.GetPendingByLocationAsync(warehouseLocationId, report.InventoryLocationId);

			if (assignment == null)
			{
				assignment = new ShipmentAssignment
				{
					Id = Guid.NewGuid(),
					WarehouseLocationId = warehouseLocationId,
					Status = AssignmentStatus.Pending,
					CreatedAt = _dateTime.UtcNow,
					CreatedByUserId = currentUser.UserId
				};
				await _assignmentRepository.AddAsync(assignment);
			}

			// FIX: Dùng bảng trung gian N-N thay vì gán trực tiếp ID vào Report
			if (!assignment.AssignmentDamageReports.Any(x => x.DamageReportId == damageReportId))
			{
				assignment.AssignmentDamageReports.Add(new AssignmentDamageReport
				{
					DamageReportId = damageReportId
				});
			}

			UpdateAssignmentType(assignment);
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
			await _notificationService.CreateInternalNotificationAsync(
				new InternalCreateNotificationRequest
				{
					UserId = request.ShipperId,
					Title = "Bạn có nhiệm vụ mới",
					Content = $"Bạn đã được phân công một nhiệm vụ vận chuyển",
					RefType = "Assignment",
					RefId = assignment.Id
				}
			);
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

			var warehouseManagers = await _userRepository
					.GetWarehouseManagersByWarehouseIdAsync(warehouseId);

			await NotifyUsersAsync(
				warehouseManagers,
				"Shipper đã nhận nhiệm vụ",
				$"Shipper đã chấp nhận nhiệm vụ {assignment.Id}",
				"Assignment",
				assignment.Id
			);
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

			var warehouseManagers = await _userRepository
				.GetWarehouseManagersByWarehouseIdAsync(warehouseId);

			await NotifyUsersAsync(
				warehouseManagers,
				"Shipper từ chối nhiệm vụ",
				$"Shipper đã từ chối nhiệm vụ {assignment.Id}",
				"Assignment",
				assignment.Id
			);
		}


		// ================= GET MY ASSIGNMENTS =================
		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetMyAssignments(
				ICurrentUser currentUser,
				AssignmentType? type,
				AssignmentStatus? status)
		{
			var assignments = await _assignmentRepository
				.GetByShipperIdWithOrderAsync(currentUser.UserId, type, status);

			return assignments.Select(MapToResponse);
		}


		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetByStoreOrderId(Guid storeOrderId)
		{
			var assignments = await _assignmentRepository
				.GetByStoreOrderIdWithDetailsAsync(storeOrderId);

			return assignments.Select(MapToResponse);
		}

		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetByShelfOrderId(Guid shelfOrderId)
		{
			var assignments = await _assignmentRepository
				.GetByShelfOrderIdWithDetailsAsync(shelfOrderId);

			return assignments.Select(MapToResponse);
		}

		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetByDamageReportId(Guid damageReportId)
		{
			var assignments = await _assignmentRepository
				.GetByDamageReportIdWithDetailsAsync(damageReportId);

			return assignments.Select(MapToResponse);
		}

		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetAllAsync(
				AssignmentType? type,
				AssignmentStatus? status,
				Guid? warehouseLocationId)
		{
			var assignments = await _assignmentRepository
				.GetAllWithDetailsAsync(type, status, warehouseLocationId);

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
			// Lấy danh sách các đơn hàng gốc để hiển thị thông tin Header
			var storeOrders = assignment.AssignmentStoreOrders?
				.Select(x => x.StoreOrder)
				.Where(x => x != null)
				.ToList() ?? new();

			var shelfOrders = assignment.AssignmentShelfOrders?
				.Select(x => x.ShelfOrder)
				.Where(x => x != null)
				.ToList() ?? new();

			var damageReports = assignment.AssignmentDamageReports?
				.Select(x => x.DamageReport)
				.Where(x => x != null)
				.ToList() ?? new();

			var shipment = assignment.Shipments?.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

			// Tạo Order Type động cho FE dễ xử lý
			var types = new List<string>();
			if (storeOrders.Any()) types.Add("STORE");
			if (shelfOrders.Any()) types.Add("SHELF");
			if (damageReports.Any()) types.Add("DAMAGE");

			// Lấy địa điểm đầu tiên để hiển thị đích đến (Destination)
			var firstStore = storeOrders.FirstOrDefault()?.StoreLocation ??
							 shelfOrders.FirstOrDefault()?.StoreLocation ??
							 damageReports.FirstOrDefault()?.InventoryLocation;

			var response = new ShipmentAssignmentResponse
			{
				Id = assignment.Id,
				OrderType = types.Any() ? string.Join("-", types) : "EMPTY",

				StoreOrders = storeOrders.Select(o => new OrderReferenceResponse { Id = o.Id, Code = o.Code }).ToList(),
				ShelfOrders = shelfOrders.Select(o => new OrderReferenceResponse { Id = o.Id, Code = o.Code }).ToList(),
				DamageReports = damageReports.Select(dr => new OrderReferenceResponse { Id = dr.Id, Code = dr.Code }).ToList(),

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

			// GOM SẢN PHẨM: Lấy từ bảng PHÂN BỔ CHI TIẾT của Admin
			response.ProductItems = (assignment.AssignmentStoreOrders ?? new List<AssignmentStoreOrder>())
				.SelectMany(aso => aso.AssignmentStoreOrderItems ?? new List<AssignmentStoreOrderItem>())
				.Select(item => new ShipmentAssignmentProductItemResponse
				{
					StoreOrderId = item.AssignmentStoreOrder?.StoreOrderId ?? Guid.Empty,
					ProductColorId = item.StoreOrderItem?.ProductColorId ?? Guid.Empty,
					SKU = item.StoreOrderItem?.ProductColor?.Product?.SKU ?? "N/A",
					ProductName = item.StoreOrderItem?.ProductColor?.Product?.Name ?? "Unknown",
					Color = item.StoreOrderItem?.ProductColor?.Color?.Name ?? "N/A",
					ImageUrl = item.StoreOrderItem?.ProductColor?.ImageUrl,

					// Số lượng Admin yêu cầu bốc trong chuyến này
					Quantity = item.AllocatedQuantity,
					// Tổng số lượng của đơn hàng gốc
					OriginalQuantity = item.StoreOrderItem?.Quantity ?? 0,
					// Số lượng đã thực hiện xong (Fulfilled)
					FulfilledQuantity = item.StoreOrderItem?.FulfilledQuantity ?? 0
				}).ToList();

			// GOM KỆ: Lấy từ bảng PHÂN BỔ CHI TIẾT (Kèm thông số kỹ thuật)
			response.ShelfItems = (assignment.AssignmentShelfOrders ?? new List<AssignmentShelfOrder>())
				.SelectMany(aso => aso.AssignmentShelfOrderItems ?? new List<AssignmentShelfOrderItem>())
				.Select(item => {
					var originalOrderItem = item.ShelfOrderItem;
					var shelfInfo = originalOrderItem?.ShelfType;

					return new ShipmentAssignmentShelfItemResponse
					{
						ShelfOrderId = item.AssignmentShelfOrder?.ShelfOrderId ?? Guid.Empty,

						ShelfTypeId = originalOrderItem?.ShelfTypeId ?? Guid.Empty,
						ShelfTypeName = originalOrderItem?.ShelfTypeName ?? "Unknown",
						ImageUrl = originalOrderItem?.ImageUrl,

						Width = shelfInfo?.Width ?? 0,
						Height = shelfInfo?.Height ?? 0,
						Depth = shelfInfo?.Depth ?? 0,
						TotalLevels = shelfInfo?.TotalLevels ?? 0,

						// Số lượng kệ Admin yêu cầu bốc
						Quantity = item.AllocatedQuantity,
						// Tổng số lượng kệ trong đơn gốc
						OriginalQuantity = originalOrderItem?.Quantity ?? 0,
						FulfilledQuantity = originalOrderItem?.FulfilledQuantity ?? 0,
					};
				}).ToList();

			// GOM THU HỒI (Damage Reports)
			response.DamageReturnItems = (damageReports ?? new List<DamageReport>())
				.SelectMany(dr => dr.Items.Select(item => new ShipmentAssignmentDamageItemResponse
				{
					DamageReportId = dr.Id,
					DamageCode = dr.Code,
					DamageType = item.DamageItemType.ToString(),
					Source = dr.Source.ToString(),
					Description = dr.Description,

					TargetName = item.DamageItemType == DamageItemType.Product
						? $"{item.ProductColor?.Product?.Name} ({item.ProductColor?.Color?.Name})"
						: $"Kệ: {item.Shelf?.Code}",

					Quantity = item.Quantity ?? 1,

					ImageUrl = item.DamageMedia?.FirstOrDefault()?.MediaUrl
							   ?? item.ProductColor?.ImageUrl
				})).ToList();

			return response;
		}
	}
}
