using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceTableApply.Request;
using ToyShelf.Application.Models.PriceTableApply.Response;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class CommissionTableApplyService : ICommissionTableApplyService
	{
		private readonly ICommissionTableApplyRepository _repo;
		private readonly IPartnerRepository _partnerRepo;       
		private readonly ICommissionTableRepository _priceTableRepo; 
		private readonly IUnitOfWork _unitOfWork;

		public CommissionTableApplyService(ICommissionTableApplyRepository repo, IPartnerRepository partnerRepo, ICommissionTableRepository priceTableRepo, IUnitOfWork unitOfWork)
		{
			_repo = repo;
			_partnerRepo = partnerRepo;
			_priceTableRepo = priceTableRepo;
			_unitOfWork = unitOfWork;
		}

		public async Task<CommissionTableApplyResponse> CreateAsync(Models.PriceTableApply.Request.CommissionTableApply request)
		{
			if (request.EndDate.HasValue && request.StartDate >= request.EndDate.Value)
			{
				throw new ArgumentException("Start day must earlier than end day.");
			}

			// 2. Validate Khóa ngoại (Có tồn tại không?)
			// (Ông tự thêm hàm ExistsAsync vào Repo gốc nhé, hoặc dùng GetById != null)
			var partner = await _partnerRepo.GetByIdAsync(request.PartnerId);
			if (partner == null) throw new AppException("Partner not found", 404);

			var table = await _priceTableRepo.GetByIdAsync(request.PriceTableId);
			if (table == null) throw new AppException("Price Table not found", 404);

			// 3. CHECK TRÙNG LỊCH (QUAN TRỌNG NHẤT)
			bool isOverlap = await _repo.HasOverlapAsync(request.PartnerId, table.Type, request.StartDate, request.EndDate);
			if (isOverlap)
			{
				throw new InvalidOperationException($"Partner '{partner.CompanyName}' đã có bảng giá [{table.Type}] áp dụng trong khoảng thời gian này rồi. Vui lòng kiểm tra lại.");
			}

			// 4. Tạo mới
			var apply = new Domain.Entities.CommissionTableApply
			{
				Id = Guid.NewGuid(),
				PartnerId = request.PartnerId,
				CommissionTableId = request.PriceTableId,
				Name = request.Name,
				StartDate = request.StartDate,
				EndDate = request.EndDate,
				IsActive = true
			};

			await _repo.AddAsync(apply);
			await _unitOfWork.SaveChangesAsync();

			// Return kết quả
			return MapToResponse(apply, partner.CompanyName, table.Name);
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var apply = await _repo.GetByIdAsync(id)
				?? throw new KeyNotFoundException("Apply record not found");

			_repo.Remove(apply);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DisablePriceTableApply(Guid id)
		{
			var price = await _repo.GetByIdAsync(id);
			if (price == null)
				throw new AppException($"PriceApply Id = {id} not found", 404);
			if (!price.IsActive)
				throw new AppException("Price table apply already inactive", 400);
			price.IsActive = false;
			_repo.Update(price);
			 await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<CommissionTableApplyResponse>> GetAllAsync(bool? isActive)
		{
			var list = await _repo.GetAllWithDetailsAsync(isActive);
			return list.Select(x => MapToResponse(
				   x,
				   x.Partner?.CompanyName,
				   x.CommissionTable?.Name
                   ));

		}

		public async Task<bool> RestorePriceTableApplyAsync(Guid id)
		{
			var price = await _repo.GetByIdAsync(id);
			if(price == null)
				throw new AppException($"PriceApply Id = {id} not found", 404);
			if (price.IsActive)
				throw new AppException("Price table apply already active", 400);
			price.IsActive = true;
			_repo.Update(price);
			await _unitOfWork.SaveChangesAsync();
			return true;


		}

		public async Task<CommissionTableApplyResponse> UpgradePartnerTierAsync(Guid partnerId, Guid newTierId)
		{
			var partner = await _partnerRepo.GetByIdAsync(partnerId)
		?? throw new AppException("Not Found Partner", 404);

			if (partner.PartnerTierId == newTierId)
				throw new AppException("Have already have Tier !", 400);

			// 2. Lấy Bảng giá mặc định của Hạng mới (Gọi qua Interface, giấu nhẹm DB đi)
			var newTierTable = await _priceTableRepo.GetActiveByTierTableAsync(newTierId)
				?? throw new AppException("Not found commission table for this tier!", 404);

			var currentTime = DateTime.UtcNow;

			// 3. Lấy các Bảng giá Hạng CŨ đang chạy & "Chốt sổ" tụi nó
			var activeTierApplies = await _repo.GetActiveTierAppliesAsync(partnerId);

			foreach (var oldApply in activeTierApplies)
			{
			
				oldApply.EndDate = currentTime;
				_repo.Update(oldApply);
			}

			// 4. Tạo Apply mới
			var newApply = new Domain.Entities.CommissionTableApply
			{
				Id = Guid.NewGuid(),
				PartnerId = partnerId,
				CommissionTableId = newTierTable.Id,
				Name = $"[Auto-Apply] Nâng cấp hạng {newTierTable.PartnerTier.Name} - Áp dụng bảng {newTierTable.Name}",
				StartDate = currentTime,
				EndDate = null,
				IsActive = true
			};

			await _repo.AddAsync(newApply);

			// 5. Nâng cấp Partner
			partner.PartnerTierId = newTierId;
			_partnerRepo.Update(partner);

			// 6. Lưu toàn bộ giao dịch
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(newApply, partner.CompanyName, newTierTable.Name);
		}
		

		private static CommissionTableApplyResponse MapToResponse(Domain.Entities.CommissionTableApply entity, string? pName, string? tName)
		{
			

			return new CommissionTableApplyResponse
			{
				Id = entity.Id,
				PartnerId = entity.PartnerId,
				PartnerName = pName ?? "Unknown",
				PriceTableId = entity.CommissionTableId,
				PriceTableName = tName ?? "Unknown",
				Name = entity.Name,
				IsActive = entity.IsActive,
				StartDate = entity.StartDate,
				EndDate = entity.EndDate,

			};
		}

	
	}
}
