using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Warehouse.Request;
using ToyCabin.Application.Models.Warehouse.Response;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class WarehouseService : IWarehouseService
	{
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public WarehouseService(
			IWarehouseRepository warehouseRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_warehouseRepository = warehouseRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}
		// ================= CREATE =================
		public async Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest request)
		{
			var warehouse = new Warehouse
			{
				Id = Guid.NewGuid(),
				Code = request.Code.Trim(),
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
			var warehouse = await _warehouseRepository.GetByIdAsync(id);
			if (warehouse == null)
				throw new AppException($"Warehouse not found. Id = {id}", 404);

			return MapToResponse(warehouse);
		}

		// ================= UPDATE =================
		public async Task<WarehouseResponse> UpdateAsync(Guid id, UpdateWarehouseRequest request)
		{
			var warehouse = await _warehouseRepository.GetByIdAsync(id);
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
			var warehouse = await _warehouseRepository.GetByIdAsync(id);
			if (warehouse == null)
				throw new AppException("Warehouse not found", 404);

			warehouse.IsActive = false;
			warehouse.UpdatedAt = _dateTime.UtcNow;

			_warehouseRepository.Update(warehouse);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task RestoreAsync(Guid id)
		{
			var warehouse = await _warehouseRepository.GetByIdAsync(id);
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
			var warehouse = await _warehouseRepository.GetByIdAsync(id);
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
				CreatedAt = warehouse.CreatedAt,
				UpdatedAt = warehouse.UpdatedAt
			};
		}
	}
}
