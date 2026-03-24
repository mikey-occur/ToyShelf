using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.StoreCreationRequest.Request;
using ToyShelf.Application.Models.StoreCreationRequest.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class StoreCreationRequestService : IStoreCreationRequestService
	{
		private readonly IStoreCreationRequestRepository _requestRepository;
		private readonly IStoreRepository _storeRepository;
		private readonly IPartnerRepository _partnerRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly IInventoryLocationRepository _inventoryLocationRepository;

		private const string Prefix = "STORE";

		public StoreCreationRequestService(
			IStoreCreationRequestRepository requestRepository,
			IStoreRepository storeRepository,
			IPartnerRepository partnerRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime,
			IInventoryLocationRepository inventoryLocationRepository)
		{
			_requestRepository = requestRepository;
			_storeRepository = storeRepository;
			_partnerRepository = partnerRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_inventoryLocationRepository = inventoryLocationRepository;
		}

		public async Task<StoreCreationRequestResponse> CreateAsync(
			CreateStoreCreationRequest request,
			ICurrentUser currentUser)
		{
			if (!currentUser.IsPartnerAdmin())
				throw new ForbiddenException("Only PartnerAdmin can create store request");

			var entity = new StoreCreationRequest
			{
				Id = Guid.NewGuid(),
				PartnerId = currentUser.PartnerId!.Value,
				Name = request.Name.Trim(),
				StoreAddress = request.StoreAddress.Trim(),
				Latitude = request.Latitude,
				Longitude = request.Longitude,
				PhoneNumber = request.PhoneNumber,
				Status = StoreRequestStatus.Pending,
				RequestedByUserId = currentUser.UserId,
				CreatedAt = _dateTime.UtcNow
			};

			await _requestRepository.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(entity);
		}


		// ================= GET =================
		public async Task<IEnumerable<StoreCreationRequestResponse>> GetRequestsAsync(StoreRequestStatus? status)
		{
			var requests = await _requestRepository.GetRequestsAsync(status);

			return requests.Select(MapToResponse);
		}

		public async Task<StoreCreationRequestResponse> GetByIdAsync(Guid id)
		{
			var entity = await _requestRepository.GetByIdAsync(id);

			if (entity == null)
				throw new AppException($"Store creation request not found. Id = {id}", 404);

			return MapToResponse(entity);
		}

		// ================= REVIEW =================
		public async Task ReviewAsync(
		Guid id,
		ReviewStoreCreationRequest request,
		ICurrentUser currentUser)
		{
			if (!currentUser.IsAdmin())
				throw new ForbiddenException("Only Admin can review store request");

			var entity = await _requestRepository.GetByIdAsync(id);

			if (entity == null)
				throw new AppException("Store creation request not found", 404);

			if (entity.Status != StoreRequestStatus.Pending)
				throw new AppException("Request already reviewed", 400);

			entity.Status = request.Status;
			entity.ReviewedByUserId = currentUser.UserId;
			entity.ReviewedAt = _dateTime.UtcNow;

			if (request.Status == StoreRequestStatus.Rejected)
			{
				if (string.IsNullOrWhiteSpace(request.RejectReason))
					throw new AppException("Reject reason is required", 400);

				entity.RejectReason = request.RejectReason;
			}

			if (request.Status == StoreRequestStatus.Approved)
			{
				var partner = await _partnerRepository.GetByIdAsync(entity.PartnerId);

				if (partner == null)
					throw new AppException("Partner not found", 404);

				var maxNumber = await _storeRepository
					.GetMaxSequenceByPartnerAsync(entity.PartnerId);

				var code = $"{Prefix}-{partner.Code}-{(maxNumber + 1):D2}";

				bool exists = await _storeRepository
					.ExistsByCodeInPartnerAsync(code, entity.PartnerId);

				if (exists)
					throw new InvalidOperationException("Store code already exists in this partner.");

				var store = new Store
				{
					Id = Guid.NewGuid(),
					PartnerId = entity.PartnerId,
					Code = code,
					Name = entity.Name,
					StoreAddress = entity.StoreAddress,
					Latitude = entity.Latitude,
					Longitude = entity.Longitude,
					PhoneNumber = entity.PhoneNumber,
					IsActive = true,
					CreatedAt = _dateTime.UtcNow
				};

				await _storeRepository.AddAsync(store);

				var location = new InventoryLocation
				{
					Id = Guid.NewGuid(),
					StoreId = store.Id,
					Type = InventoryLocationType.Store,
					Name = store.Name,
					IsActive = true
				};

				await _inventoryLocationRepository.AddAsync(location);
			}

			_requestRepository.Update(entity);
			await _unitOfWork.SaveChangesAsync();
		}


		// ================= DELETE =================
		public async Task DeleteAsync(Guid id)
		{
			var entity = await _requestRepository.GetByIdAsync(id);

			if (entity == null)
				throw new AppException("Store creation request not found", 404);

			_requestRepository.Remove(entity);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= MAPPER =================
		private static StoreCreationRequestResponse MapToResponse(StoreCreationRequest entity)
		{
			return new StoreCreationRequestResponse
			{
				Id = entity.Id,
				PartnerId = entity.PartnerId,
				Name = entity.Name,
				StoreAddress = entity.StoreAddress,
				Latitude = entity.Latitude,
				Longitude = entity.Longitude,
				PhoneNumber = entity.PhoneNumber,
				Status = entity.Status,
				RequestedByUserId = entity.RequestedByUserId,
				ReviewedByUserId = entity.ReviewedByUserId,
				RejectReason = entity.RejectReason,
				CreatedAt = entity.CreatedAt,
				ReviewedAt = entity.ReviewedAt
			};
		}
	}
}
