using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PartnerTier.Request;
using ToyShelf.Application.Models.PartnerTier.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class PartnerTierService : IPartnerTierService
	{
		private readonly IPartnerTierRepository _repo;
		private readonly IUnitOfWork _unitOfWork;

		public PartnerTierService(IPartnerTierRepository repo, IUnitOfWork unitOfWork)
		{
			_repo = repo;
			_unitOfWork = unitOfWork;
		}
		public async Task<PartnerTierResponse> CreateAsync(PartnerTierRequest request)
		{
		
			if (await _repo.ExistsByNameAsync(request.Name))
				throw new AppException($"Tier Name '{request.Name}' already exists.", 400);

			if (await _repo.ExistsByPriorityAsync(request.Priority))
				throw new AppException($"Priority '{request.Priority}' is already assigned to another tier.", 400);

			var tier = new PartnerTier
			{
				Id = Guid.NewGuid(),
				Name = request.Name.Trim(),
				Priority = request.Priority,
				MaxShelvesPerStore = request.MaxShelvesPerStore
			};

			await _repo.AddAsync(tier);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(tier);
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var tier = await _repo.GetByIdAsync(id) ?? throw new AppException("Partner Tier not found", 404);

		
			bool inUse = await _repo.IsTierInUseAsync(id);
			if (inUse)
				throw new AppException("Cannot delete this Tier because it is assigned to existing Partners.", 400);

			_repo.Remove(tier);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<PartnerTierResponse>> GetAllAsync()
		{
			var list = await _repo.GetAllAsync();
			return list.OrderBy(x => x.Priority).Select(MapToResponse);
		}

		public async Task<PartnerTierResponse> GetByIdAsync(Guid id)
		{
			var tier = await _repo.GetByIdAsync(id) ?? throw new AppException("Partner Tier not found", 404);
			return MapToResponse(tier);
		}

		public async Task<PartnerTierResponse> UpdateAsync(Guid id, PartnerTierRequest request)
		{
			var tier = await _repo.GetByIdAsync(id) ?? throw new AppException("Partner Tier not found", 404);

			if (tier.Name != request.Name && await _repo.ExistsByNameAsync(request.Name, id))
				throw new AppException($"Tier Name '{request.Name}' already exists.", 400);

			if (tier.Priority != request.Priority && await _repo.ExistsByPriorityAsync(request.Priority, id))
				throw new AppException($"Priority '{request.Priority}' is already assigned.", 400);

			tier.MaxShelvesPerStore = request.MaxShelvesPerStore;

			tier.Name = request.Name.Trim();
			tier.Priority = request.Priority;
			tier.MaxShelvesPerStore = request.MaxShelvesPerStore;

			_repo.Update(tier);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(tier);
		}

		private static PartnerTierResponse MapToResponse(PartnerTier tier)
		{
			return new PartnerTierResponse
			{
				Id = tier.Id,
				Name = tier.Name,
				Priority = tier.Priority,
				MaxShelvesPerStore = tier.MaxShelvesPerStore
			};
		}
	}
}
