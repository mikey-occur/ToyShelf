using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.DamageReport.Request;
using ToyShelf.Application.Models.DamageReport.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class DamageReportService : IDamageReportService
	{
		private readonly IDamageReportRepository _repository;
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IShelfRepository _shelfRepository;
		private readonly IInventoryLocationRepository _locationRepository;
		private readonly IUserStoreRepository _userStoreRepository;
		private readonly IShipmentAssignmentService _shipmentAssignmentService;
		private readonly IInventoryShelfRepository _inventoryShelfRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		private const string Prefix = "DR";

		public DamageReportService(
			IDamageReportRepository repository,
			IInventoryRepository inventoryRepository,
			IShelfRepository shelfRepository,
			IInventoryLocationRepository locationRepository,
			IUserStoreRepository userStoreRepository,
			IShipmentAssignmentService shipmentAssignmentService,
			IInventoryShelfRepository inventoryShelfRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_repository = repository;
			_inventoryRepository = inventoryRepository;
			_shelfRepository = shelfRepository;
			_locationRepository = locationRepository;
			_userStoreRepository = userStoreRepository;
			_shipmentAssignmentService = shipmentAssignmentService;
			_inventoryShelfRepository = inventoryShelfRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		private void UpdateReportType(DamageReport report)
		{
			bool hasProduct = report.Items.Any(i => i.DamageItemType == DamageItemType.Product);
			bool hasShelf = report.Items.Any(i => i.DamageItemType == DamageItemType.Shelf);

			if (hasProduct && hasShelf) report.Type = DamageReportType.Combined;
			else if (hasShelf) report.Type = DamageReportType.Shelf;
			else report.Type = DamageReportType.Product;
		}

		// ================= GET =================
		public async Task<IEnumerable<DamageReportResponse>> GetAllAsync(DamageStatus? status, Guid? partnerId, Guid? storeId )
		{
			var reports = await _repository.GetAllWithIncludeAsync(status, partnerId, storeId);
			return reports.Select(MapToResponse);
		}

		public async Task<DamageReportResponse> GetByIdAsync(Guid id)
		{
			var report = await _repository.GetByIdFullIncludeAsync(id);
			if (report == null) throw new AppException("Damage report not found", 404);
			return MapToResponse(report);
		}

		// ================= CREATE (Store gửi yêu cầu Trả hàng/Bảo hành) =================
		public async Task<DamageReportResponse> CreateAsync(CreateDamageReportRequest request, ICurrentUser currentUser)
		{
			if (request.Items == null || !request.Items.Any())
				throw new AppException("Vui lòng thêm ít nhất một sản phẩm hoặc kệ bị hỏng", 400);

			var userStore = await _userStoreRepository.GetByUserIdAsync(currentUser.UserId);
			if (userStore == null || !userStore.IsActive)
				throw new AppException("User is not assigned to any active store", 403);

			var location = await _locationRepository.GetStoreLocationByStoreIdAsync(userStore.StoreId);
			if (location == null) throw new AppException("Store inventory location not found", 404);

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var maxNumber = await _repository.GetMaxSequenceAsync();
				var code = $"{Prefix}-{_dateTime.UtcNow:yyyyMMdd}-{(maxNumber + 1):D5}";

				var report = new DamageReport
				{
					Id = Guid.NewGuid(),
					Code = code,
					Source = request.Source,
					Status = DamageStatus.Pending,
					InventoryLocationId = location.Id,
					Description = request.Description,
					IsWarrantyClaim = request.IsWarrantyClaim,
					ReportedByUserId = currentUser.UserId,
					CreatedAt = _dateTime.UtcNow
				};

				// --- BƯỚC 1: XỬ LÝ SẢN PHẨM (Giữ nguyên logic cũ) ---
				var productItems = request.Items.Where(x => x.Type == DamageItemType.Product).ToList();
				foreach (var itemReq in productItems)
				{
					var item = new DamageReportItem
					{
						Id = Guid.NewGuid(),
						DamageReportId = report.Id,
						DamageItemType = itemReq.Type,
						ProductColorId = itemReq.ProductColorId,
						Quantity = itemReq.Quantity
					};

					var invAvail = await _inventoryRepository.GetByLocationAndProductAsync(location.Id, itemReq.ProductColorId!.Value, InventoryStatus.Available);
					if (invAvail == null || invAvail.Quantity < item.Quantity)
						throw new AppException($"Sản phẩm mã {itemReq.ProductColorId} không đủ tồn kho để báo hỏng", 400);

					invAvail.Quantity -= item.Quantity ?? 0;

					var invPending = await _inventoryRepository.GetByLocationAndProductAsync(location.Id, itemReq.ProductColorId.Value, InventoryStatus.PendingDamaged);
					if (invPending == null)
					{
						invPending = new Inventory { Id = Guid.NewGuid(), InventoryLocationId = location.Id, ProductColorId = itemReq.ProductColorId.Value, Status = InventoryStatus.PendingDamaged, Quantity = item.Quantity ?? 0 };
						await _inventoryRepository.AddAsync(invPending);
					}
					else { invPending.Quantity += item.Quantity ?? 0; }

					AddMedia(item, itemReq.MediaUrls);
					report.Items.Add(item);
				}

				// --- BƯỚC 2: XỬ LÝ KỆ (Sửa logic GroupBy để tránh Duplicate Key) ---
				var shelfItemsReq = request.Items.Where(x => x.Type == DamageItemType.Shelf).ToList();

				// Lấy thông tin chi tiết các kệ trước
				var shelfIds = shelfItemsReq.Select(x => x.ShelfId!.Value).ToList();
				var shelves = await _shelfRepository.GetByIds(shelfIds);

				// Group theo ShelfTypeId để cập nhật InventoryShelf (Sổ cái) 1 lần duy nhất mỗi loại
				var shelfGroups = shelves.GroupBy(s => s.ShelfTypeId);

				foreach (var group in shelfGroups)
				{
					var shelfTypeId = group.Key;
					var quantityInGroup = group.Count();

					// 1. Cập nhật InUse (Trừ tổng số lượng trong group)
					var invInUse = await _inventoryShelfRepository.GetShelfWithStatusAsync(location.Id, shelfTypeId, ShelfStatus.InUse);
					if (invInUse == null || invInUse.Quantity < quantityInGroup)
						throw new AppException($"Không đủ số lượng kệ 'InUse' trong kho cho loại kệ này", 400);

					invInUse.RemoveQuantity(quantityInGroup);

					// 2. Cập nhật PendingMaintenance (Cộng tổng số lượng vào 1 dòng duy nhất)
					var invPendingMaint = await _inventoryShelfRepository.GetShelfWithStatusAsync(location.Id, shelfTypeId, ShelfStatus.PendingMaintenance);
					if (invPendingMaint == null)
					{
						invPendingMaint = new InventoryShelf(location.Id, shelfTypeId, quantityInGroup, ShelfStatus.PendingMaintenance);
						await _inventoryShelfRepository.AddAsync(invPendingMaint);
					}
					else { invPendingMaint.AddQuantity(quantityInGroup); }

					// 3. Cập nhật từng thực thể kệ vật lý và tạo DamageReportItem
					foreach (var shelf in group)
					{
						if (shelf.Status != ShelfStatus.InUse)
							throw new AppException($"Kệ {shelf.Id} không ở trạng thái InUse", 400);

						shelf.Status = ShelfStatus.PendingMaintenance;

						var item = new DamageReportItem
						{
							Id = Guid.NewGuid(),
							DamageReportId = report.Id,
							DamageItemType = DamageItemType.Shelf,
							ShelfId = shelf.Id,
							Quantity = 1
						};

						// Lấy media từ request gốc tương ứng với shelfId này
						var originalReq = shelfItemsReq.First(x => x.ShelfId == shelf.Id);
						AddMedia(item, originalReq.MediaUrls);

						report.Items.Add(item);
					}
				}

				UpdateReportType(report);
				await _repository.AddAsync(report);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				var full = await _repository.GetByIdFullIncludeAsync(report.Id);
				return MapToResponse(full!);
			}
			catch { await _unitOfWork.RollbackTransactionAsync(); throw; }
		}

		// Hàm phụ trợ để giữ nguyên logic Media cũ
		private void AddMedia(DamageReportItem item, List<string>? urls)
		{
			if (urls != null)
			{
				foreach (var url in urls)
				{
					item.DamageMedia.Add(new DamageMedia
					{
						Id = Guid.NewGuid(),
						MediaUrl = url,
						MediaType = "IMAGE",
						CreatedAt = DateTime.UtcNow
					});
				}
			}
		}

		public async Task PartnerApproveAsync(Guid id, ICurrentUser currentUser)
		{
			var report = await _repository.GetByIdAsync(id);
			if (report == null) throw new AppException("Báo cáo không tồn tại", 404);

			if (report.Status != DamageStatus.Pending)
				throw new AppException("Báo cáo phải ở trạng thái chờ (Pending)", 400);

			// Chỉ cập nhật thông tin duyệt, KHÔNG cập nhật số lượng kho ở bước này
			report.Status = DamageStatus.PartnerApproved;
			report.PartnerAdminApprovedAt = _dateTime.UtcNow;
			report.PartnerAdminApprovedByUserId = currentUser.UserId;

			_repository.Update(report);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= APPROVE (Admin duyệt xác nhận hàng hỏng) =================
		public async Task ApproveAsync(Guid id, string? adminNote, ICurrentUser currentUser)
		{
			var report = await _repository.GetByIdFullIncludeAsync(id);
			if (report == null) throw new AppException("Báo cáo hư hại không tồn tại", 404);
			if (report.Status != DamageStatus.PartnerApproved)
				throw new AppException("Báo cáo cần được phía Đối tác duyệt trước khi Admin hệ thống chốt", 400);

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// 1. Cập nhật thông tin chung của báo cáo
				report.Status = DamageStatus.Approved;
				report.AdminNote = adminNote;
				report.ReviewedByUserId = currentUser.UserId;
				report.ReviewedAt = _dateTime.UtcNow;

				// --- BƯỚC 2: XỬ LÝ SẢN PHẨM (Group theo ProductColorId) ---
				var productItems = report.Items.Where(x => x.DamageItemType == DamageItemType.Product).ToList();
				foreach (var group in productItems.GroupBy(x => x.ProductColorId))
				{
					var productColorId = group.Key!.Value;
					var totalQty = group.Sum(x => x.Quantity ?? 0);

					// Giảm số lượng ở kho Tạm (PendingDamaged)
					var invPending = await _inventoryRepository.GetByLocationAndProductAsync(
						report.InventoryLocationId, productColorId, InventoryStatus.PendingDamaged);

					if (invPending != null)
					{
						invPending.Quantity -= totalQty;
						if (invPending.Quantity < 0) invPending.Quantity = 0; // Bảo vệ số liệu không âm
						_inventoryRepository.Update(invPending);
					}

					// Tăng số lượng ở kho Hỏng chính thức (Damaged)
					var damagedInv = await _inventoryRepository.GetByLocationAndProductAsync(
						report.InventoryLocationId, productColorId, InventoryStatus.Damaged);

					if (damagedInv == null)
					{
						damagedInv = new Inventory
						{
							Id = Guid.NewGuid(),
							InventoryLocationId = report.InventoryLocationId,
							ProductColorId = productColorId,
							Status = InventoryStatus.Damaged,
							Quantity = totalQty
						};
						await _inventoryRepository.AddAsync(damagedInv);
					}
					else
					{
						damagedInv.Quantity += totalQty;
						_inventoryRepository.Update(damagedInv);
					}
				}

				// --- BƯỚC 3: XỬ LÝ KỆ (Group theo ShelfTypeId để khớp Unique Index) ---
				var shelfItems = report.Items.Where(x => x.DamageItemType == DamageItemType.Shelf).ToList();
				foreach (var group in shelfItems.GroupBy(x => x.Shelf?.ShelfTypeId))
				{
					if (group.Key == null) continue;
					var shelfTypeId = group.Key.Value;
					var count = group.Count();

					// 1. Giảm ở Sổ cái Tạm (PendingMaintenance)
					var invPendingMaint = await _inventoryShelfRepository.GetShelfWithStatusAsync(
						report.InventoryLocationId, shelfTypeId, ShelfStatus.PendingMaintenance);

					if (invPendingMaint != null)
					{
						int qtyToRemove = Math.Min(invPendingMaint.Quantity, count);
						invPendingMaint.RemoveQuantity(qtyToRemove);
						_inventoryShelfRepository.Update(invPendingMaint);
					}

					// 2. Tăng ở Sổ cái Bảo trì chính thức (Maintenance)
					var invMaintenance = await _inventoryShelfRepository.GetShelfWithStatusAsync(
						report.InventoryLocationId, shelfTypeId, ShelfStatus.Maintenance);

					if (invMaintenance == null)
					{
						invMaintenance = new InventoryShelf(report.InventoryLocationId, shelfTypeId, count, ShelfStatus.Maintenance);
						await _inventoryShelfRepository.AddAsync(invMaintenance);
					}
					else
					{
						invMaintenance.AddQuantity(count);
						_inventoryShelfRepository.Update(invMaintenance);
					}

					// 3. Cập nhật từng cái kệ vật lý
					foreach (var item in group)
					{
						if (item.Shelf != null)
						{
							item.Shelf.Status = ShelfStatus.Maintenance;
							_shelfRepository.Update(item.Shelf);
						}
					}
				}

				_repository.Update(report);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		// ================= ASSIGN RECALL (Chỉ định kho và gộp lệnh thu hồi) =================
		public async Task CreateRecallAssignmentAsync(Guid id, Guid warehouseLocationId, ICurrentUser currentUser)
		{
			var report = await _repository.GetByIdAsync(id);
			if (report == null) throw new AppException("Báo cáo không tồn tại", 404);

			// Kiểm tra: Chỉ những báo cáo đã duyệt (Approved) mới được phép đi thu hồi
			if (report.Status != DamageStatus.Approved)
				throw new AppException("Báo cáo phải được duyệt (Approved) trước khi tạo lệnh thu hồi", 400);

			await _shipmentAssignmentService.CreateFromDamageReportAsync(report.Id, warehouseLocationId, currentUser);

			await _unitOfWork.SaveChangesAsync();
		}

		// ================= REJECT (Admin từ chối báo cáo hàng hỏng) =================
		public async Task RejectAsync(Guid id, string? adminNote, ICurrentUser currentUser)
		{
			var report = await _repository.GetByIdFullIncludeAsync(id);
			if (report == null) throw new AppException("Báo cáo hư hại không tồn tại", 404);
			if (report.Status != DamageStatus.Pending && report.Status != DamageStatus.PartnerApproved)
				throw new AppException("Báo cáo đã ở trạng thái không thể từ chối", 400);

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// 1. Cập nhật thông tin chung của báo cáo
				report.Status = DamageStatus.Rejected;
				report.AdminNote = adminNote;
				report.ReviewedByUserId = currentUser.UserId;
				report.ReviewedAt = _dateTime.UtcNow;

				// --- BƯỚC 2: XỬ LÝ SẢN PHẨM (Group theo ProductColorId) ---
				var productItems = report.Items.Where(x => x.DamageItemType == DamageItemType.Product).ToList();
				foreach (var group in productItems.GroupBy(x => x.ProductColorId))
				{
					var productColorId = group.Key!.Value;
					var totalQty = group.Sum(x => x.Quantity ?? 0);

					// Giảm ở kho Tạm (PendingDamaged)
					var invPending = await _inventoryRepository.GetByLocationAndProductAsync(
						report.InventoryLocationId, productColorId, InventoryStatus.PendingDamaged);

					if (invPending != null)
					{
						invPending.Quantity -= totalQty;
						if (invPending.Quantity < 0) invPending.Quantity = 0;
						_inventoryRepository.Update(invPending);
					}

					// Trả về kho Sẵn sàng (Available)
					var invAvail = await _inventoryRepository.GetByLocationAndProductAsync(
						report.InventoryLocationId, productColorId, InventoryStatus.Available);

					if (invAvail == null)
					{
						invAvail = new Inventory
						{
							Id = Guid.NewGuid(),
							InventoryLocationId = report.InventoryLocationId,
							ProductColorId = productColorId,
							Status = InventoryStatus.Available,
							Quantity = totalQty
						};
						await _inventoryRepository.AddAsync(invAvail);
					}
					else
					{
						invAvail.Quantity += totalQty;
						_inventoryRepository.Update(invAvail);
					}
				}

				// --- BƯỚC 3: XỬ LÝ KỆ (Group theo ShelfTypeId để tránh Duplicate Key) ---
				var shelfItems = report.Items.Where(x => x.DamageItemType == DamageItemType.Shelf).ToList();
				foreach (var group in shelfItems.GroupBy(x => x.Shelf?.ShelfTypeId))
				{
					if (group.Key == null) continue;
					var shelfTypeId = group.Key.Value;
					var count = group.Count();

					// 1. Giảm ở Sổ cái Tạm (PendingMaintenance)
					var invPendingMaint = await _inventoryShelfRepository.GetShelfWithStatusAsync(
						report.InventoryLocationId, shelfTypeId, ShelfStatus.PendingMaintenance);

					if (invPendingMaint != null)
					{
						int qtyToRemove = Math.Min(invPendingMaint.Quantity, count);
						invPendingMaint.RemoveQuantity(qtyToRemove);
						_inventoryShelfRepository.Update(invPendingMaint);
					}

					// 2. Trả về Sổ cái Đang sử dụng (InUse)
					var invInUse = await _inventoryShelfRepository.GetShelfWithStatusAsync(
						report.InventoryLocationId, shelfTypeId, ShelfStatus.InUse);

					if (invInUse == null)
					{
						invInUse = new InventoryShelf(report.InventoryLocationId, shelfTypeId, count, ShelfStatus.InUse);
						await _inventoryShelfRepository.AddAsync(invInUse);
					}
					else
					{
						invInUse.AddQuantity(count);
						_inventoryShelfRepository.Update(invInUse);
					}

					// 3. Trả lại trạng thái InUse cho từng cái kệ vật lý
					foreach (var item in group)
					{
						if (item.Shelf != null)
						{
							item.Shelf.Status = ShelfStatus.InUse;
							_shelfRepository.Update(item.Shelf);
						}
					}
				}

				_repository.Update(report);
				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		// ================= MAPPER =================
		private static DamageReportResponse MapToResponse(DamageReport report)
		{
			return new DamageReportResponse
			{
				Id = report.Id,
				Code = report.Code,

				ShipmentAssignmentIds = report.AssignmentDamageReports?
					.Select(adr => adr.ShipmentAssignmentId)
					.ToList() ?? new List<Guid>(),

				Type = report.Type,
				Source = report.Source,
				Status = report.Status,

				Description = report.Description,
				AdminNote = report.AdminNote,
				IsWarrantyClaim = report.IsWarrantyClaim,

				// Lấy StoreName từ InventoryLocation
				StoreName = report.InventoryLocation?.Store?.Name ?? "N/A",
				StoreAddress = report.InventoryLocation?.Store?.StoreAddress ?? "N/A",

				// Thông tin nhân sự
				ReportedByUserId = report.ReportedByUserId,
				ReportedByName = report.ReportedByUser?.FullName ?? "Unknown",

				PartnerAdminApprovedByUserId = report.PartnerAdminApprovedByUserId,
				PartnerAdminName = report.PartnerAdminApprovedByUser?.FullName ?? "Unknown",

				ReviewedByUserId = report.ReviewedByUserId,
				ReviewedByName = report.ReviewedByUser?.FullName ?? "Unknown",

				// Thời gian
				CreatedAt = report.CreatedAt,
				PartnerAdminApprovedAt = report.PartnerAdminApprovedAt,
				ReviewedAt = report.ReviewedAt,

				// Map danh sách Items lồng bên trong
				Items = report.Items.Select(i => new DamageItemResponse
				{
					Id = i.Id,
					Type = i.DamageItemType,
					Quantity = i.Quantity,

					Product = i.DamageItemType == DamageItemType.Product
				? new ProductInfo
				{
					ProductColorId = i.ProductColorId,
					ProductName = i.ProductColor?.Product?.Name,
					SKU = i.ProductColor?.Product?.SKU,
					ColorName = i.ProductColor?.Color?.Name,
					ImageUrl = i.ProductColor?.ImageUrl
				}
				: null,

					Shelf = i.DamageItemType == DamageItemType.Shelf
				? new ShelfInfo
				{
					ShelfId = i.ShelfId,
					ShelfCode = i.Shelf?.Code
				}
				: null,

					MediaUrls = i.DamageMedia.Select(m => m.MediaUrl).ToList()
				}).ToList()
			};
		}
	}
}
