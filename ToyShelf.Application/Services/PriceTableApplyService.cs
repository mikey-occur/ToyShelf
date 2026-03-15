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
	public class PriceTableApplyService : IPriceTableApplyService
	{
		private readonly IPriceTableApplyRepository _repo;
		private readonly IPartnerRepository _partnerRepo;       
		private readonly IPriceTableRepository _priceTableRepo; 
		private readonly IUnitOfWork _unitOfWork;

		public PriceTableApplyService(IPriceTableApplyRepository repo, IPartnerRepository partnerRepo, IPriceTableRepository priceTableRepo, IUnitOfWork unitOfWork)
		{
			_repo = repo;
			_partnerRepo = partnerRepo;
			_priceTableRepo = priceTableRepo;
			_unitOfWork = unitOfWork;
		}

		public async Task<PriceTableApplyResponse> CreateAsync(PriceTableApplyRequest request)
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
			bool isOverlap = await _repo.HasOverlapAsync(request.PartnerId, request.StartDate, request.EndDate);
			if (isOverlap)
			{
				throw new InvalidOperationException($"Partner '{partner.CompanyName}' đã có bảng giá áp dụng trong khoảng thời gian này rồi. Vui lòng kiểm tra lại.");
			}

			// 4. Tạo mới
			var apply = new PriceTableApply
			{
				Id = Guid.NewGuid(),
				PartnerId = request.PartnerId,
				PriceTableId = request.PriceTableId,
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

		public async Task<IEnumerable<PriceTableApplyResponse>> GetAllAsync(bool? isActive)
		{
			var list = await _repo.GetAllWithDetailsAsync(isActive);
			return list.Select(x => MapToResponse(
				   x,
				   x.Partner?.CompanyName,
				   x.PriceTable?.Name
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

		private static PriceTableApplyResponse MapToResponse(PriceTableApply entity, string? pName, string? tName)
		{
			

			return new PriceTableApplyResponse
			{
				Id = entity.Id,
				PartnerId = entity.PartnerId,
				PartnerName = pName ?? "Unknown",
				PriceTableId = entity.PriceTableId,
				PriceTableName = tName ?? "Unknown",
				Name = entity.Name,
				IsActive = entity.IsActive,
				StartDate = entity.StartDate,
				EndDate = entity.EndDate,

			};
		}
	}
}
