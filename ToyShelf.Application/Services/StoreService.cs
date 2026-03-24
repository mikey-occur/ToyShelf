using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Store.Request;
using ToyShelf.Application.Models.Store.Response;
using ToyShelf.Application.Models.User.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class StoreService : IStoreService
	{
		private readonly IStoreRepository _storeRepository;
		private readonly IPartnerRepository _partnerRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly IInventoryLocationRepository _inventoryLocationRepository;


		private const string Prefix = "STORE";

		public StoreService(
			IStoreRepository storeRepository,
			IPartnerRepository partnerRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime,
			IInventoryLocationRepository inventoryLocationRepository)
		{
			_storeRepository = storeRepository;
			_partnerRepository = partnerRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_inventoryLocationRepository = inventoryLocationRepository;
		}

		// ================= CREATE =================
		public async Task<StoreResponse> CreateAsync(CreateStoreRequest request)
		{
			var partner = await _partnerRepository.GetByIdAsync(request.PartnerId);
			if (partner == null)
				throw new AppException("Partner not found", 404);

			string finalCode;

			if (!string.IsNullOrWhiteSpace(request.Code))
			{
				finalCode = request.Code.Trim().ToUpper();
			}
			else
			{
				var maxNumber = await _storeRepository
					.GetMaxSequenceByPartnerAsync(request.PartnerId);

				finalCode = $"{Prefix}-{partner.Code}-{(maxNumber + 1):D2}";
			}

			bool exists = await _storeRepository
				.ExistsByCodeInPartnerAsync(finalCode, request.PartnerId);

			if (exists)
				throw new InvalidOperationException("Store code already exists in this partner.");

			var store = new Store
			{
				Id = Guid.NewGuid(),
				PartnerId = request.PartnerId,
				Code = finalCode,
				Name = request.Name.Trim(),
				StoreAddress = request.StoreAddress.Trim(),
				Latitude = request.Latitude,
				Longitude = request.Longitude,
				PhoneNumber = request.PhoneNumber,
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
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(store);
		}


		// ================= GET =================
		public async Task<IEnumerable<StoreResponse>> GetStoresAsync(bool? isActive)
		{
			var stores = await _storeRepository.GetStoresAsync(isActive);
			return stores.Select(MapToResponse);
		}

		public async Task<StoreResponse> GetByIdAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException($"Store not found. Id = {id}", 404);

			return MapToResponse(store);
		}

		// ================= UPDATE =================
		public async Task<StoreResponse> UpdateAsync(Guid id, UpdateStoreRequest request)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException($"Store not found. Id = {id}", 404);

			store.Name = request.Name.Trim();
			store.StoreAddress = request.StoreAddress.Trim();
			store.Latitude = request.Latitude;
			store.Longitude = request.Longitude;
			store.PhoneNumber = request.PhoneNumber;
			store.UpdatedAt = _dateTime.UtcNow;

			var location = await _inventoryLocationRepository
				.GetByStoreIdAsync(store.Id);

			if (location != null)
			{
				location.Name = store.Name;
				_inventoryLocationRepository.Update(location);
			}


			_storeRepository.Update(store);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(store);
		}

		// ================= DISABLE / RESTORE =================
		public async Task DisableAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException("Store not found", 404);

			store.IsActive = false;
			store.UpdatedAt = _dateTime.UtcNow;

			var location = await _inventoryLocationRepository
				.GetByStoreIdAsync(store.Id);

			if (location != null)
			{
				location.IsActive = false;
				_inventoryLocationRepository.Update(location);
			}


			_storeRepository.Update(store);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task RestoreAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException("Store not found", 404);

			store.IsActive = true;
			store.UpdatedAt = _dateTime.UtcNow;

			var location = await _inventoryLocationRepository
				.GetByStoreIdAsync(store.Id);

			if (location != null)
			{
				location.IsActive = true;
				_inventoryLocationRepository.Update(location);
			}


			_storeRepository.Update(store);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= DELETE =================
		public async Task DeleteAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException("Store not found", 404);

			var location = await _inventoryLocationRepository
				.GetByStoreIdAsync(store.Id);

			if (location != null)
			{
				_inventoryLocationRepository.Remove(location);
			}

			_storeRepository.Remove(store);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= MAPPER =================
		private static StoreResponse MapToResponse(Store store)
		{
			return new StoreResponse
			{
				Id = store.Id,
				PartnerId = store.PartnerId,
				Code = store.Code,
				Name = store.Name,
				StoreAddress = store.StoreAddress,
				Latitude = store.Latitude,
				Longitude = store.Longitude,
				PhoneNumber = store.PhoneNumber,
				IsActive = store.IsActive,
				CreatedAt = store.CreatedAt,
				UpdatedAt = store.UpdatedAt
			};
		}
	}
}
