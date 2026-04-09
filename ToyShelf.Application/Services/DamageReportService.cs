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
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_repository = repository;
			_inventoryRepository = inventoryRepository;
			_shelfRepository = shelfRepository;
			_locationRepository = locationRepository;
			_userStoreRepository = userStoreRepository;
			_shipmentAssignmentService = shipmentAssignmentService;
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

		// ================= CREATE (Store gửi yêu cầu Trả hàng/Bảo hành) =================
		public async Task<DamageReportResponse> CreateAsync(CreateDamageReportRequest request, ICurrentUser currentUser)
		{

			if (request.Items == null || !request.Items.Any())
				throw new AppException("Vui lòng thêm ít nhất một sản phẩm hoặc kệ bị hỏng", 400);

			// 1. Lấy store & location của user
			var userStore = await _userStoreRepository.GetByUserIdAsync(currentUser.UserId);
			if (userStore == null || !userStore.IsActive)
				throw new AppException("User is not assigned to any active store", 403);

			var location = await _locationRepository.GetStoreLocationByStoreIdAsync(userStore.StoreId);
			if (location == null)
				throw new AppException("Store inventory location not found", 404);

			// 2. Tạo Header Report
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

			// 3. Add Items từ Request
			foreach (var itemReq in request.Items)
			{
				var item = new DamageReportItem
				{
					Id = Guid.NewGuid(),
					DamageReportId = report.Id,
					DamageItemType = itemReq.Type,
					ProductColorId = itemReq.ProductColorId,
					ShelfId = itemReq.ShelfId,
					Quantity = itemReq.Type == DamageItemType.Shelf ? 1 : itemReq.Quantity
				};

				// Validate tồn kho nếu là Product
				if (itemReq.Type == DamageItemType.Product)
				{
					var inv = await _inventoryRepository.GetByLocationAndProductAsync(location.Id, itemReq.ProductColorId!.Value);
					if (inv == null || inv.Quantity < item.Quantity)
						throw new AppException($"Sản phẩm {itemReq.ProductColorId} không đủ tồn kho để báo hỏng", 400);
				}

				// Add Media cho từng Item
				if (itemReq.MediaUrls != null)
				{
					foreach (var url in itemReq.MediaUrls)
					{
						item.DamageMedia.Add(new DamageMedia
						{
							Id = Guid.NewGuid(),
							MediaUrl = url,
							MediaType = "IMAGE",
							CreatedAt = _dateTime.UtcNow
						});
					}
				}
				report.Items.Add(item);
			}

			UpdateReportType(report);
			await _repository.AddAsync(report);
			await _unitOfWork.SaveChangesAsync();

			var full = await _repository.GetByIdFullIncludeAsync(report.Id);
			return MapToResponse(full!);
		}

		// ================= GET =================
		public async Task<IEnumerable<DamageReportResponse>> GetAllAsync(DamageStatus? status)
		{
			var reports = await _repository.GetAllWithIncludeAsync(status);
			return reports.Select(MapToResponse);
		}

		public async Task<DamageReportResponse> GetByIdAsync(Guid id)
		{
			var report = await _repository.GetByIdFullIncludeAsync(id);
			if (report == null) throw new AppException("Damage report not found", 404);
			return MapToResponse(report);
		}

		// ================= APPROVE (Admin quyết định kho thu hồi) =================
		public async Task ApproveAsync(Guid id, Guid warehouseLocationId, string? adminNote, ICurrentUser currentUser)
		{
			var report = await _repository.GetByIdFullIncludeAsync(id);
			if (report == null) throw new AppException("Report not found", 404);
			if (report.Status != DamageStatus.Pending) throw new AppException("Report already processed", 400);

			report.Status = DamageStatus.Approved;
			report.AdminNote = adminNote;
			report.ReviewedByUserId = currentUser.UserId;
			report.ReviewedAt = _dateTime.UtcNow;

			// Xử lý từng Item trong đơn
			foreach (var item in report.Items)
			{
				if (item.DamageItemType == DamageItemType.Product)
				{
					var storeInv = await _inventoryRepository.GetByLocationAndProductAsync(report.InventoryLocationId, item.ProductColorId!.Value);
					if (storeInv != null)
					{
						storeInv.Quantity -= item.Quantity ?? 0;
						_inventoryRepository.Update(storeInv);
					}
				}
				else if (item.DamageItemType == DamageItemType.Shelf)
				{
					var shelf = await _shelfRepository.GetByIdAsync(item.ShelfId!.Value);
					if (shelf != null)
					{
						shelf.Status = ShelfStatus.Maintenance;
						_shelfRepository.Update(shelf);
					}
				}
			}

			_repository.Update(report);

			// Luồng điều phối vận chuyển thu hồi
			await _shipmentAssignmentService.CreateFromDamageReportAsync(report.Id, warehouseLocationId, currentUser);

			await _unitOfWork.SaveChangesAsync();
		}

		// ================= REJECT =================
		public async Task RejectAsync(Guid id, string? adminNote, ICurrentUser currentUser)
		{
			var report = await _repository.GetByIdAsync(id);
			if (report == null) throw new AppException("Report not found", 404);
			if (report.Status != DamageStatus.Pending) throw new AppException("Report already processed", 400);

			report.Status = DamageStatus.Rejected;
			report.AdminNote = adminNote;
			report.ReviewedByUserId = currentUser.UserId;
			report.ReviewedAt = _dateTime.UtcNow;

			_repository.Update(report);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= MAPPER =================
		private static DamageReportResponse MapToResponse(DamageReport report)
		{
			return new DamageReportResponse
			{
				Id = report.Id,
				Code = report.Code,
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
				ReviewedByUserId = report.ReviewedByUserId,
				ReviewedByName = report.ReviewedByUser?.FullName ?? "Unknown",

				// Thời gian
				CreatedAt = report.CreatedAt,
				ReviewedAt = report.ReviewedAt,

				// Map danh sách Items lồng bên trong
				Items = report.Items.Select(i => new DamageItemResponse
				{
					Id = i.Id,
					Type = i.DamageItemType,
					Quantity = i.Quantity,

					// Product info
					ProductColorId = i.ProductColorId,
					ProductName = i.ProductColor?.Product?.Name,
					SKU = i.ProductColor?.Product?.SKU,
					ColorName = i.ProductColor?.Color?.Name,
					ImageUrl = i.ProductColor?.ImageUrl,

					// Shelf info
					ShelfId = i.ShelfId,
					ShelfCode = i.Shelf?.Code,

					// Media theo món
					MediaUrls = i.DamageMedia.Select(m => m.MediaUrl).ToList()
				}).ToList()
			};
		}
	}
}
