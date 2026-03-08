using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.InventoryDisposition.Request;
using ToyShelf.Application.Models.InventoryDisposition.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class InventoryDispositionService : IInventoryDispositionService
	{
		private readonly IInventoryDispositionRepository _repository;
		private readonly IUnitOfWork _unitOfWork;

		public InventoryDispositionService(
			IInventoryDispositionRepository repository,
			IUnitOfWork unitOfWork)
		{
			_repository = repository;
			_unitOfWork = unitOfWork;
		}

		// ========= CREATE =========
		public async Task<InventoryDispositionResponse> CreateAsync(CreateInventoryDispositionRequest request)
		{
			var code = request.Code.Trim().ToUpper();

			var exists = await _repository.AnyAsync(x => x.Code == code);

			if (exists)
				throw new AppException("Inventory disposition code already exists");

			var entity = new InventoryDisposition
			{
				Id = Guid.NewGuid(),
				Code = code,
				Description = request.Description?.Trim()
			};

			await _repository.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(entity);
		}

		// ========= GET ALL =========
		public async Task<IEnumerable<InventoryDispositionResponse>> GetAllAsync()
		{
			var list = await _repository.GetAllAsync();

			return list.Select(MapToResponse);
		}

		// ========= GET BY ID =========
		public async Task<InventoryDispositionResponse> GetByIdAsync(Guid id)
		{
			var entity = await _repository.GetByIdAsync(id);

			if (entity == null)
				throw new AppException("Inventory disposition not found", 404);

			return MapToResponse(entity);
		}

		// ========= UPDATE =========
		public async Task<InventoryDispositionResponse> UpdateAsync(
			Guid id,
			UpdateInventoryDispositionRequest request)
		{
			var entity = await _repository.GetByIdAsync(id);

			if (entity == null)
				throw new AppException("Inventory disposition not found", 404);

			var code = request.Code.Trim().ToUpper();

			bool exists = await _repository.AnyAsync(x => x.Code == code && x.Id != id);

			if (exists)
				throw new AppException("Inventory disposition code already exists");

			entity.Code = code;
			entity.Description = request.Description?.Trim();

			_repository.Update(entity);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(entity);
		}

		// ========= DELETE =========
		public async Task DeleteAsync(Guid id)
		{
			var entity = await _repository.GetByIdAsync(id);

			if (entity == null)
				throw new AppException("Inventory disposition not found", 404);

			_repository.Remove(entity);

			await _unitOfWork.SaveChangesAsync();
		}

		// ========= MAPPER =========
		private static InventoryDispositionResponse MapToResponse(InventoryDisposition entity)
		{
			return new InventoryDispositionResponse
			{
				Id = entity.Id,
				Code = entity.Code,
				Description = entity.Description
			};
		}
	}
}
