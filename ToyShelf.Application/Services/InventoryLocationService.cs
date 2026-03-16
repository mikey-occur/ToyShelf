using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.InventoryLocation.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class InventoryLocationService : IInventoryLocationService
	{
		private readonly IInventoryLocationRepository _inventoryLocationRepository;
		private readonly IUnitOfWork _unitOfWork;

		public InventoryLocationService(
			IInventoryLocationRepository inventoryLocationRepository,
			IUnitOfWork unitOfWork)
		{
			_inventoryLocationRepository = inventoryLocationRepository;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<InventoryLocationResponse>> GetInventoryLocationsAsync(bool? isActive, Guid? StoreId, Guid? WarehouseId)
		{
			var locations = await _inventoryLocationRepository
				.GetInventoryLocationsAsync(isActive, StoreId, WarehouseId);

			return locations.Select(MapToResponse);
		}


		public async Task<InventoryLocationResponse> GetByIdAsync(Guid id)
		{
			var location = await _inventoryLocationRepository
				.GetByIdAsync(id);

			if (location == null)
				throw new AppException($"InventoryLocation not found. Id = {id}", 404);

			return MapToResponse(location);
		}

		public async Task DisableAsync(Guid id)
		{
			var location = await _inventoryLocationRepository.GetByIdAsync(id);

			if (location == null)
				throw new AppException("InventoryLocation not found", 404);

			location.IsActive = false;

			_inventoryLocationRepository.Update(location);
			await _unitOfWork.SaveChangesAsync();
		}


		public async Task RestoreAsync(Guid id)
		{
			var location = await _inventoryLocationRepository.GetByIdAsync(id);

			if (location == null)
				throw new AppException("InventoryLocation not found", 404);

			location.IsActive = true;

			_inventoryLocationRepository.Update(location);
			await _unitOfWork.SaveChangesAsync();
		}

		private static InventoryLocationResponse MapToResponse(InventoryLocation location)
		{
			return new InventoryLocationResponse
			{
				Id = location.Id,
				Type = location.Type,
				WarehouseId = location.WarehouseId,
				StoreId = location.StoreId,
				Name = location.Name,
				IsActive = location.IsActive,
			};
		}
	}
}
