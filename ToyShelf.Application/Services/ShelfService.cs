using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shelf.Request;
using ToyShelf.Application.Models.Shelf.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
    public class ShelfService : IShelfService
    {
        private readonly IShelfRepository _shelfRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public ShelfService(
            IShelfRepository shelfRepository,
            IStoreRepository storeRepository,
            IPartnerRepository partnerRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime)
        {
            _shelfRepository = shelfRepository;
            _storeRepository = storeRepository;
            _partnerRepository = partnerRepository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        // ===== CREATE =====
        public async Task<ShelfResponse> CreateAsync(CreateShelfRequest request)
        {
            if (request.PartnerId.HasValue)
            {
                var partner = await _partnerRepository.GetByIdAsync(request.PartnerId.Value);
                if (partner == null)
                    throw new AppException("Partner not found", 404);
            }

            if (request.StoreId.HasValue)
            {
                var store = await _storeRepository.GetByIdAsync(request.StoreId.Value);
                if (store == null)
                    throw new AppException("Store not found", 404);
            }

            var shelf = new Shelf
            {
                Id = Guid.NewGuid(),
                PartnerId = request.PartnerId,
                StoreId = request.StoreId,
                Code = request.Code.Trim(),
                Level = request.Level,
                Status = ShelfStatus.Available,
                AssignedAt = request.StoreId.HasValue ? _dateTime.UtcNow : null
            };

            await _shelfRepository.AddAsync(shelf);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(shelf);
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
            var shelf = await _shelfRepository.GetByIdAsync(id);
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

            if (request.PartnerId.HasValue && request.PartnerId != shelf.PartnerId)
            {
                var partner = await _partnerRepository.GetByIdAsync(request.PartnerId.Value);
                if (partner == null)
                    throw new AppException("Partner not found", 404);
            }

            if (request.StoreId.HasValue && request.StoreId != shelf.StoreId)
            {
                var store = await _storeRepository.GetByIdAsync(request.StoreId.Value);
                if (store == null)
                    throw new AppException("Store not found", 404);
            }

            shelf.PartnerId = request.PartnerId;
            shelf.StoreId = request.StoreId;
            shelf.Code = request.Code.Trim();
            shelf.Level = request.Level;
            shelf.Status = request.Status;

            _shelfRepository.Update(shelf);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponse(shelf);
        }

        // ===== GET PAGINATED =====
        public async Task<PaginatedResult<ShelfResponse>> GetPaginatedAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            ShelfStatus? shelfStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ShelfStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                shelfStatus = parsedStatus;
            }

            var (items, totalCount) = await _shelfRepository.GetShelvesPaginatedAsync(pageNumber, pageSize, shelfStatus);

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

        // ===== MAPPER =====
        private static ShelfResponse MapToResponse(Shelf shelf)
        {
            return new ShelfResponse
            {
                Id = shelf.Id,
                PartnerId = shelf.PartnerId,
                StoreId = shelf.StoreId,
                Code = shelf.Code,
                Level = shelf.Level,
                Status = shelf.Status,
                AssignedAt = shelf.AssignedAt,
                UnassignedAt = shelf.UnassignedAt
            };
        }
    }
}
