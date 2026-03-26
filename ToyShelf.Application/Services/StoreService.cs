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
		private readonly IUserRepository _userRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly IInventoryLocationRepository _inventoryLocationRepository;


		private const string Prefix = "STORE";

		public StoreService(
			IStoreRepository storeRepository,
			IPartnerRepository partnerRepository,
			IUserRepository userRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime,
			IInventoryLocationRepository inventoryLocationRepository)
		{
			_storeRepository = storeRepository;
			_partnerRepository = partnerRepository;
			_userRepository = userRepository;
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
				CityId = request.CityId,
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


			var createdStore = await _storeRepository
				.GetByIdWithDetailsAsync(store.Id);

			var partnerAdmin = await _userRepository
				.GetPartnerAdminByPartnerIdAsync(createdStore!.PartnerId);

			return MapToResponse(createdStore!, partnerAdmin);
		}


		// ================= GET =================
		public async Task<IEnumerable<StoreResponse>> GetStoresAsync(
			bool? isActive,
			Guid? companyId,
			string? keyword,
			Guid? cityId)
		{
			var stores = await _storeRepository
				.GetStoresAsync(isActive, companyId, keyword, cityId);

			var result = new List<StoreResponse>();

			foreach (var store in stores)
			{
				var partnerAdmin = await _userRepository
					.GetPartnerAdminByPartnerIdAsync(store.PartnerId);

				result.Add(MapToResponse(store, partnerAdmin));
			}

			return result;
		}

		public async Task<StoreResponse> GetByIdAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdWithDetailsAsync(id);
			if (store == null)
				throw new AppException($"Store not found. Id = {id}", 404);

			var partnerAdmin = await _userRepository
				.GetPartnerAdminByPartnerIdAsync(store.PartnerId);

			return MapToResponse(store, partnerAdmin);
		}


		// ================= UPDATE =================
		public async Task<StoreResponse> UpdateAsync(Guid id, UpdateStoreRequest request)
		{
			var store = await _storeRepository.GetByIdWithDetailsAsync(id);
			if (store == null)
				throw new AppException($"Store not found. Id = {id}", 404);

			store.Name = request.Name.Trim();
			store.StoreAddress = request.StoreAddress.Trim();
			store.CityId = request.CityId;
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

			var partnerAdmin = await _userRepository
				.GetPartnerAdminByPartnerIdAsync(store.PartnerId);

			return MapToResponse(store, partnerAdmin);
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
		private static StoreResponse MapToResponse(Store store, User? partnerAdmin)
		{
			var location = store.InventoryLocations
				.FirstOrDefault(l => l.Type == InventoryLocationType.Store);

			return new StoreResponse
			{
				Id = store.Id,
				PartnerId = store.PartnerId,
				PartnerName = store.Partner?.CompanyName ?? string.Empty,

				InventoryLocationId = location?.Id ?? Guid.Empty,

				Code = store.Code,
				Name = store.Name,
				StoreAddress = store.StoreAddress,

				CityId = store.CityId,
				CityName = store.City?.Name ?? string.Empty,

				OwnerId = partnerAdmin?.Id,
				OwnerName = partnerAdmin?.FullName ?? string.Empty,

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
