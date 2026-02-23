using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.CommissionPolicy.Request;
using ToyShelf.Application.Models.CommissionPolicy.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class CommissionPolicyService : ICommissionPolicyService
	{
		private readonly ICommissionPolicyRepository _repo;
		private readonly IUnitOfWork _unitOfWork;

		public CommissionPolicyService(ICommissionPolicyRepository repo, IUnitOfWork unitOfWork)
		{
			_repo = repo;
			_unitOfWork = unitOfWork;
		}
		public async Task<CommissionPolicyResponse> CreateAsync(CommissionPolicyRequest request)
		{
			var existing = await _repo.GetByTierAndSegmentAsync(request.PartnerTierId, request.PriceSegmentId);
			if (existing != null)
			{
				throw new InvalidOperationException($"Policy for Tier and Segment already exists (ID: {existing.Id}). Please update instead.");
			}

		
			var policy = new CommissionPolicy
			{
				Id = Guid.NewGuid(),
				PartnerTierId = request.PartnerTierId,
				PriceSegmentId = request.PriceSegmentId,
				CommissionRate = request.CommissionRate,
				EffectiveDate = request.EffectiveDate ?? DateTime.UtcNow
			};

			await _repo.AddAsync(policy);
			await _unitOfWork.SaveChangesAsync();

			// Load lại đầy đủ để lấy tên hiển thị
			var fullPolicy = await _repo.GetByTierAndSegmentAsync(policy.PartnerTierId, policy.PriceSegmentId);
			return MapToResponse(fullPolicy!);
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var policy = await _repo.GetByIdAsync(id)
				?? throw new KeyNotFoundException("Not found");

			_repo.Remove(policy);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<CommissionPolicyResponse>> GetAllAsync()
		{
			var list = await _repo.GetAllWithDetailsAsync();
			return list.Select(MapToResponse);
		}

		public async Task<CommissionPolicyResponse> GetByIdAsync(Guid id)
		{
		
			var policy = await _repo.GetByIdAsync(id);
			if (policy == null) throw new KeyNotFoundException("Not found");
			var fullPolicy = await _repo.GetByTierAndSegmentAsync(policy.PartnerTierId, policy.PriceSegmentId);
			return MapToResponse(fullPolicy!);
		}

		public async Task<IEnumerable<CommissionPolicyResponse>> GetByTierIdAsync(Guid tierId)
		{
			var list = await _repo.GetByTierIdAsync(tierId);
			return list.Select(MapToResponse);
		}

		public async Task<CommissionPolicyResponse> UpdateAsync(Guid id, UpdateCommissionPolicyRequest request)
		{
			// 1. Kiểm tra xem Policy có tồn tại không
			var policy = await _repo.GetByIdAsync(id);

			if (policy == null)
			{
				throw new KeyNotFoundException($"Không tìm thấy Commission Policy với ID: {id}");
			}
			policy.CommissionRate = request.CommissionRate;
			policy.EffectiveDate = request.EffectiveDate ?? DateTime.UtcNow;
			_repo.Update(policy);
			await _unitOfWork.SaveChangesAsync();
			var fullPolicy = await _repo.GetByTierAndSegmentAsync(policy.PartnerTierId, policy.PriceSegmentId);
			return MapToResponse(fullPolicy!);
		}

		private static CommissionPolicyResponse MapToResponse(CommissionPolicy entity)
		{
			return new CommissionPolicyResponse
			{
				Id = entity.Id,
				PartnerTierId = entity.PartnerTierId,
				PartnerTierName = entity.PartnerTier?.Name ?? "Unknown Tier",
				PriceSegmentId = entity.PriceSegmentId,
				PriceSegmentName = entity.PriceSegment?.Name ?? "Unknown Segment",
				CommissionRate = entity.CommissionRate,
				EffectiveDate = entity.EffectiveDate
			};
		}
	}
}
