using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Warehouse.Request;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class WarehouseService : IWarehouseService
	{
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly ICityRepository _cityRepository;

		private const string Prefix = "WH";

		public WarehouseService(
			IWarehouseRepository warehouseRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime,
			ICityRepository cityRepository)
		{
			_warehouseRepository = warehouseRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_cityRepository = cityRepository;
		}
		// ================= CREATE =================
		public async Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest request)
		{
			// Kiểm tra City tồn tại
			var city = await _cityRepository.GetByIdAsync(request.CityId);
			if (city == null)
				throw new AppException("City not found.", 404);

			string finalCode;

			// Nếu admin nhập Code
			if (!string.IsNullOrWhiteSpace(request.Code))
			{
				finalCode = request.Code.Trim().ToUpper();
			}
			else
			{
				var maxNumber = await _warehouseRepository
					.GetMaxSequenceByCityAsync(request.CityId);

				finalCode = $"{Prefix}-{city.Code}-{(maxNumber + 1):D2}";
			}

			// Kiểm tra trùng Code trong cùng City
			bool exists = await _warehouseRepository
				.ExistsByCodeInCityAsync(finalCode, request.CityId);

			if (exists)
				throw new InvalidOperationException("Warehouse code already exists in this city.");

			var warehouse = new Warehouse
			{
				Id = Guid.NewGuid(),
				CityId = request.CityId,
				Code = finalCode,
				Name = request.Name.Trim(),
				Address = request.Address?.Trim(),
				IsActive = true,
				CreatedAt = _dateTime.UtcNow
			};

			await _warehouseRepository.AddAsync(warehouse);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(warehouse);
		}


		// ================= GET =================
		public async Task<IEnumerable<WarehouseResponse>> GetWarehousesAsync(bool? isActive)
		{
			var warehouses = await _warehouseRepository.GetWarehousesAsync(isActive);
			return warehouses.Select(MapToResponse);
		}

		public async Task<WarehouseResponse> GetByIdAsync(Guid id)
		{
			var warehouse = await _warehouseRepository.GetByIdWithCityAsync(id);
			if (warehouse == null)
				throw new AppException($"Warehouse not found. Id = {id}", 404);

			return MapToResponse(warehouse);
		}

		// ================= UPDATE =================
		public async Task<WarehouseResponse> UpdateAsync(Guid id, UpdateWarehouseRequest request)
		{
			var warehouse = await _warehouseRepository.GetByIdWithCityAsync(id);
			if (warehouse == null)
				throw new AppException($"Warehouse not found. Id = {id}", 404);

			warehouse.Name = request.Name.Trim();
			warehouse.Address = request.Address?.Trim();
			warehouse.UpdatedAt = _dateTime.UtcNow;

			_warehouseRepository.Update(warehouse);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(warehouse);
		}

		// ================= DISABLE / RESTORE =================
		public async Task DisableAsync(Guid id)
		{
			var warehouse = await _warehouseRepository.GetByIdWithCityAsync(id);
			if (warehouse == null)
				throw new AppException("Warehouse not found", 404);

			warehouse.IsActive = false;
			warehouse.UpdatedAt = _dateTime.UtcNow;

			_warehouseRepository.Update(warehouse);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task RestoreAsync(Guid id)
		{
			var warehouse = await _warehouseRepository.GetByIdWithCityAsync(id);
			if (warehouse == null)
				throw new AppException("Warehouse not found", 404);

			warehouse.IsActive = true;
			warehouse.UpdatedAt = _dateTime.UtcNow;

			_warehouseRepository.Update(warehouse);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= DELETE =================
		public async Task DeleteAsync(Guid id)
		{
			var warehouse = await _warehouseRepository.GetByIdWithCityAsync(id);
			if (warehouse == null)
				throw new AppException("Warehouse not found", 404);

			_warehouseRepository.Remove(warehouse);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= MAPPER =================
		private static WarehouseResponse MapToResponse(Warehouse warehouse)
		{
			return new WarehouseResponse
			{
				Id = warehouse.Id,
				Code = warehouse.Code,
				Name = warehouse.Name,
				Address = warehouse.Address,
				IsActive = warehouse.IsActive,
				
				CityId = warehouse.CityId,
				CityName = warehouse.City.Name,
				CityCode = warehouse.City.Code,

				CreatedAt = warehouse.CreatedAt,
				UpdatedAt = warehouse.UpdatedAt
			};
		}
	}
}
