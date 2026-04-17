using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shelf.Request;
using ToyShelf.Application.Models.Shelf.Response;
using ToyShelf.Application.Models.ShelfType.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
    public class ShelfService : IShelfService
    {
        private readonly IShelfRepository _shelfRepository;
        private readonly IInventoryLocationRepository _inventoryLocationRepository; 
		private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
		private readonly IInventoryShelfService _inventoryShelfService;
		public ShelfService(
            IShelfRepository shelfRepository,
            IInventoryLocationRepository inventoryLocationRepository,
			IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
			IInventoryShelfService inventoryShelfService)
        {
            _shelfRepository = shelfRepository;
            _inventoryLocationRepository = inventoryLocationRepository;
			_unitOfWork = unitOfWork;
            _dateTime = dateTime;
			_inventoryShelfService = inventoryShelfService;
        }

		// ===== CREATE =====
		public async Task<List<ShelfResponse>> CreateAsync(CreateShelfRequest request)
		{
			if (request.Quantity <= 0)
				throw new AppException("Số lượng kệ tạo mới phải lớn hơn 0!", 400);

			await _unitOfWork.BeginTransactionAsync();

			try
			{
				var location = await _inventoryLocationRepository.GetByIdAsync(request.InventoryLocationId);
				if (location == null) throw new AppException("Không tìm thấy kho!", 404);

				if (location.Type != InventoryLocationType.Warehouse)
					throw new AppException("Kệ mới bắt buộc phải nhập tại Kho Tổng!", 400);

				// 1. Lấy mã cuối cùng để biết bắt đầu từ đâu
				var lastCode = await _shelfRepository.GetLastShelfCodeAsync();
				int lastNumber = 0;
				if (!string.IsNullOrEmpty(lastCode))
				{
					// "SH-00010" -> 10
					int.TryParse(lastCode.Replace("SH-", ""), out lastNumber);
				}

				var createdShelves = new List<Shelf>();

				
				for (int i = 1; i <= request.Quantity; i++)
				{
					var shelf = new Shelf
					{
						Id = Guid.NewGuid(),
						InventoryLocationId = request.InventoryLocationId,
						ShelfTypeId = request.ShelfTypeId,
						Code = $"SH-{(lastNumber + i):D5}", // Ví dụ: SH-00011
						Status = ShelfStatus.Available
					};

					await _shelfRepository.AddAsync(shelf);
					createdShelves.Add(shelf);
				}

				// 3. Lưu bước 1 (Bảng Shelf)
				await _unitOfWork.SaveChangesAsync();

				// 4. Cập nhật số lượng vào bảng InventoryShelf (Cờ check xung đột Version/xmin nằm ở đây)
				await _inventoryShelfService.AddShelfQuantityAsync(
					request.InventoryLocationId,
					request.ShelfTypeId,
					request.Quantity);

				await _unitOfWork.CommitTransactionAsync();

				var ids = createdShelves.Select(x => x.Id).ToList();
				var shelvesWithData = await _shelfRepository.GetByIdsWithDetailsAsync(ids);

				return shelvesWithData.Select(s => MapToResponse(s)).ToList();
			}
			catch (Exception)
			{
				await _unitOfWork.RollbackTransactionAsync();
				throw;
			}
		}

		// ===== GET ALL =====
		public async Task<IEnumerable<ShelfResponse>> GetAllAsync()
		{
			var shelves = await _shelfRepository.GetAllAsync();
			return shelves.Select(MapToResponse);
		}

		// ===== GET BY ID =====
		public async Task<ShelfResponse> GetByIdAsync(Guid id)
		{
			var shelf = await _shelfRepository.GetByIdWithDetailsAsync(id);
			if (shelf == null)
				throw new AppException($"Shelf not found. Id = {id}", 404);

			return MapToResponse(shelf);
		}

		// ===== UPDATE =====
		public async Task<ShelfResponse> UpdateAsync(Guid id, UpdateShelfRequest request)
		{
			var shelf = await _shelfRepository.GetByIdAsync(id);
			if (shelf == null)
				throw new AppException($"Shelf not found. Id = {id}", 404);

			if (request.InventoryLocationId != shelf.InventoryLocationId)
			{
				var location = await _inventoryLocationRepository.GetByIdAsync(request.InventoryLocationId);
				if (location == null)
					throw new AppException("Inventory location not found", 404);
			}

			shelf.InventoryLocationId = request.InventoryLocationId;
			shelf.Code = request.Code.Trim();
			shelf.ShelfTypeId = request.ShelfTypeId;
			shelf.Status = request.Status;

			_shelfRepository.Update(shelf);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(shelf);
		}

		// ===== GET PAGINATED =====
		public async Task<PaginatedResult<ShelfResponse>> GetPaginatedAsync(
			int pageNumber = 1,
			int pageSize = 10,
			string? status = null,
			Guid? inventoryLocationId = null)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1) pageSize = 10;

			ShelfStatus? shelfStatus = null;
			if (!string.IsNullOrEmpty(status) &&
				Enum.TryParse<ShelfStatus>(status, true, out var parsed))
			{
				shelfStatus = parsed;
			}

			var (items, totalCount) = await _shelfRepository.GetShelvesPaginatedAsync(
				pageNumber,
				pageSize,
				shelfStatus,
				inventoryLocationId);

			return new PaginatedResult<ShelfResponse>
			{
				Items = items.Select(MapToResponse),
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize
			};
		}

		// ===== DELETE =====
		public async Task DeleteAsync(Guid id)
        {
            var shelf = await _shelfRepository.GetByIdAsync(id);
            if (shelf == null)
                throw new AppException($"Shelf not found. Id = {id}", 404);

            _shelfRepository.Remove(shelf);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ShelfResponse> UpdateShelftStatus(Guid id, ShelfStatus newStatus)
        {
            var shelf = await _shelfRepository.GetByIdAsync(id);
            if (shelf == null)
                throw new AppException($"Shelf not found. Id = {id}", 404);
            shelf.Status = newStatus;

            if (newStatus == ShelfStatus.Recalled)
            {
                shelf.UnassignedAt = _dateTime.UtcNow;
            }
            else if (newStatus == ShelfStatus.InUse)
            {
                shelf.AssignedAt = _dateTime.UtcNow;
            }
            _shelfRepository.Update(shelf);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(shelf);
        }

      


        // ===== MAPPER =====
        private static ShelfResponse MapToResponse(Shelf shelf)
        {
            return new ShelfResponse
            {
                Id = shelf.Id,
				InventoryLocationId = shelf.InventoryLocationId,
				Code = shelf.Code,
                ShelfTypeId = shelf.ShelfTypeId,
                Status = shelf.Status,
                AssignedAt = shelf.AssignedAt,
                UnassignedAt = shelf.UnassignedAt,
                ShelfType = shelf.ShelfType != null ? MapToShelfTypeResponse(shelf.ShelfType) : null
            };

        }

        private static ShelfTypeResponse MapToShelfTypeResponse(ShelfType shelfType)
        {
            return new ShelfTypeResponse
            {
                Id = shelfType.Id,
                Name = shelfType.Name,
                ImageUrl = shelfType.ImageUrl,
                Width = shelfType.Width,
                Height = shelfType.Height,
                Depth = shelfType.Depth,
                TotalLevels = shelfType.TotalLevels,
                SuitableProductCategoryTypes = string.IsNullOrWhiteSpace(shelfType.SuitableProductCategoryTypes) ? new List<string>() : shelfType.SuitableProductCategoryTypes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                DisplayGuideline = shelfType.DisplayGuideline,
                IsActive = shelfType.IsActive,

                Levels = shelfType.ShelfTypeLevels?.Select(l => new ShelfTypeResponse.ShelfTypeLevelResponse
                {
                    Level = l.Level,
                    Name = l.Name,
                    ClearanceHeight = l.ClearanceHeight,
                    RecommendedCapacity = l.RecommendedCapacity,
                    SuitableProductCategoryTypes = string.IsNullOrWhiteSpace(l.SuitableProductCategoryTypes) ? new List<string>() : l.SuitableProductCategoryTypes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    DisplayGuideline = l.DisplayGuideline
                }).ToList() ?? new List<ShelfTypeResponse.ShelfTypeLevelResponse>()
            };
        }
    }
}
