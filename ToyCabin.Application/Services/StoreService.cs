using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Store.Request;
using ToyCabin.Application.Models.Store.Response;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class StoreService : IStoreService
	{
		private readonly IStoreRepository _storeRepository;
		private readonly IPartnerRepository _partnerRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public StoreService(
			IStoreRepository storeRepository,
			IPartnerRepository partnerRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_storeRepository = storeRepository;
			_partnerRepository = partnerRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// ================= CREATE =================
		public async Task<StoreResponse> CreateAsync(CreateStoreRequest request)
		{
			var partner = await _partnerRepository.GetByIdAsync(request.PartnerId);
			if (partner == null)
				throw new AppException("Partner not found", 404);

			var store = new Store
			{
				Id = Guid.NewGuid(),
				PartnerId = request.PartnerId,
				Code = request.Code.Trim(),
				Name = request.Name.Trim(),
				StoreAddress = request.StoreAddress.Trim(),
				PhoneNumber = request.PhoneNumber,
				IsActive = true,
				CreatedAt = _dateTime.UtcNow
			};

			await _storeRepository.AddAsync(store);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(store);
		}

		// ================= GET =================
		public async Task<IEnumerable<StoreResponse>> GetAllAsync()
		{
			var stores = await _storeRepository.GetAllAsync();
			return stores.Select(MapToResponse);
		}

		public async Task<IEnumerable<StoreResponse>> GetActiveAsync()
		{
			var stores = await _storeRepository.FindAsync(s => s.IsActive);
			return stores.Select(MapToResponse);
		}

		public async Task<IEnumerable<StoreResponse>> GetInactiveAsync()
		{
			var stores = await _storeRepository.FindAsync(s => !s.IsActive);
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
			store.PhoneNumber = request.PhoneNumber;
			store.UpdatedAt = _dateTime.UtcNow;

			_storeRepository.Update(store);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(store);
		}

		// ================= DISABLE / RESTORE =================
		public async Task<bool> DisableAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException("Store not found", 404);

			store.IsActive = false;
			store.UpdatedAt = _dateTime.UtcNow;

			_storeRepository.Update(store);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<bool> RestoreAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException("Store not found", 404);

			store.IsActive = true;
			store.UpdatedAt = _dateTime.UtcNow;

			_storeRepository.Update(store);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		// ================= DELETE =================
		public async Task<bool> DeleteAsync(Guid id)
		{
			var store = await _storeRepository.GetByIdAsync(id);
			if (store == null)
				throw new AppException("Store not found", 404);

			_storeRepository.Remove(store);
			await _unitOfWork.SaveChangesAsync();
			return true;
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
				PhoneNumber = store.PhoneNumber,
				IsActive = store.IsActive,
				CreatedAt = store.CreatedAt,
				UpdatedAt = store.UpdatedAt
			};
		}
	}
}
