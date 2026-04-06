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
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		private const string Prefix = "DR";

		public DamageReportService(
			IDamageReportRepository repository,
			IInventoryRepository inventoryRepository,
			IShelfRepository shelfRepository,
			IInventoryLocationRepository locationRepository,
			IUserStoreRepository userStoreRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_repository = repository;
			_inventoryRepository = inventoryRepository;
			_shelfRepository = shelfRepository;
			_locationRepository = locationRepository;
			_userStoreRepository = userStoreRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// ================= CREATE (Store gửi yêu cầu Trả hàng/Bảo hành) =================
		public async Task<DamageReportResponse> CreateAsync(CreateDamageReportRequest request, ICurrentUser currentUser)
		{
			// 1. Lấy store của user
			var userStore = await _userStoreRepository.GetByUserIdAsync(currentUser.UserId);
			if (userStore == null || !userStore.IsActive)
				throw new AppException("User is not assigned to any active store", 403);

			// 2. Lấy InventoryLocation của store
			var location = await _locationRepository.GetStoreLocationByStoreIdAsync(userStore.StoreId);
			if (location == null)
				throw new AppException("Store inventory location not found", 404);

			// 3. Kiểm tra logic theo loại hư hại (Product hoặc Shelf)
			if (request.Type == DamageType.Product)
			{
				if (!request.ProductColorId.HasValue) throw new AppException("ProductColorId is required", 400);

				var inventory = await _inventoryRepository.GetByLocationAndProductAsync(location.Id, request.ProductColorId.Value);
				if (inventory == null || inventory.Quantity < request.Quantity)
					throw new AppException("Not enough inventory to report damage", 400);
			}
			else if (request.Type == DamageType.Shelf)
			{
				if (!request.ShelfId.HasValue) throw new AppException("ShelfId is required", 400);
			}

			// 4. Tạo Code (DR-yyyyMMdd-XXXXX)
			var maxNumber = await _repository.GetMaxSequenceAsync();
			var code = $"{Prefix}-{_dateTime.UtcNow:yyyyMMdd}-{(maxNumber + 1):D5}";

			var report = new DamageReport
			{
				Id = Guid.NewGuid(),
				Code = code,
				Type = request.Type,
				Source = request.Source,
				Status = DamageStatus.Pending,
				InventoryLocationId = location.Id,
				ProductColorId = request.ProductColorId,
				ShelfId = request.ShelfId,
				Quantity = request.Type == DamageType.Shelf ? 1 : request.Quantity,
				Description = request.Description,
				IsWarrantyClaim = request.IsWarrantyClaim,
				ReportedByUserId = currentUser.UserId,
				CreatedAt = _dateTime.UtcNow
			};

			// 5. Thêm Media bằng chứng
			if (request.MediaUrls != null && request.MediaUrls.Any())
			{
				foreach (var url in request.MediaUrls)
				{
					report.DamageMedia.Add(new DamageMedia
					{
						Id = Guid.NewGuid(),
						MediaUrl = url,
						MediaType = "IMAGE",
						CreatedAt = _dateTime.UtcNow
					});
				}
			}

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

		// ================= APPROVE (Duyệt thu hồi/Bảo hành) =================
		public async Task ApproveAsync(Guid id, string? adminNote, ICurrentUser currentUser)
		{
			var report = await _repository.GetByIdAsync(id);
			if (report == null) throw new AppException("Report not found", 404);
			if (report.Status != DamageStatus.Pending) throw new AppException("Report already processed", 400);

			report.Status = DamageStatus.Approved;
			report.AdminNote = adminNote;
			report.ReviewedByUserId = currentUser.UserId;
			report.ReviewedAt = _dateTime.UtcNow;

			// Xử lý thực tế sau khi duyệt
			if (report.Type == DamageType.Product)
			{
				// Trừ kho Available tại Store
				var inventory = await _inventoryRepository.GetByLocationAndProductAsync(report.InventoryLocationId, report.ProductColorId!.Value);
				if (inventory != null)
				{
					inventory.Quantity -= report.Quantity;
					// Note: Bạn có thể chuyển sang bản ghi InventoryStatus.Damaged nếu hệ thống yêu cầu tách kho lỗi
				}
			}
			else if (report.Type == DamageType.Shelf)
			{
				// Khóa kệ, chuyển trạng thái sang Bảo trì/Thu hồi
				var shelf = await _shelfRepository.GetByIdAsync(report.ShelfId!.Value);
				if (shelf != null)
				{
					shelf.Status = ShelfStatus.Maintenance;
					_shelfRepository.Update(shelf);
				}
			}

			_repository.Update(report);
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

				ProductColorId = report.ProductColorId,
				ProductName = report.ProductColor?.Product?.Name ?? "",
				SKU = report.ProductColor?.Product?.SKU ?? "",
				ColorName = report.ProductColor?.Color?.Name ?? "",
				ImageUrl = report.ProductColor?.ImageUrl,

				ShelfId = report.ShelfId,
				ShelfCode = report.Shelf?.Code ?? "",

				Quantity = report.Quantity,
				Description = report.Description,
				AdminNote = report.AdminNote,
				IsWarrantyClaim = report.IsWarrantyClaim,

				ReportedByUserId = report.ReportedByUserId,
				ReportedByName = report.ReportedByUser?.FullName ?? "",
				ReviewedByUserId = report.ReviewedByUserId,
				ReviewedByName = report.ReviewedByUser?.FullName ?? "",

				CreatedAt = report.CreatedAt,
				ReviewedAt = report.ReviewedAt,

				MediaUrls = report.DamageMedia.Select(m => m.MediaUrl).ToList()
			};
		}
	}
}
