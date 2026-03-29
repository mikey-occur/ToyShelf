using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Partner.Request;
using ToyShelf.Application.Models.Partner.Response;
using ToyShelf.Application.Models.Store.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class PartnerService : IPartnerService
	{
		private readonly IPartnerRepository _partnerRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly ICommissionTableRepository _CommissiontableRepo;
		private readonly ICommissionTableApplyRepository _commissionTableApplyRepo;
		private readonly ICommissionTableApplyService _commissionTableApplyService;
		public PartnerService(
			IPartnerRepository partnerRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime,
			ICommissionTableRepository commissiontableRepo,
			ICommissionTableApplyRepository commissionTableApplyRepo,
			ICommissionTableApplyService commissionTableApplyService)
		{
			_partnerRepository = partnerRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_CommissiontableRepo = commissiontableRepo;
			_commissionTableApplyRepo = commissionTableApplyRepo;
			_commissionTableApplyService = commissionTableApplyService;
		}

		private async Task<string> GeneratePartnerCode(string companyName)
		{
			// Lấy chữ cái đầu
			var prefix = string.Concat(
				companyName
				.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.Select(w => char.ToUpper(w[0]))
			);

			// Lấy các code bắt đầu bằng prefix
			var partners = await _partnerRepository.GetByCodePrefixAsync(prefix);

			int nextNumber = 1;

			if (partners.Any())
			{
				var maxNumber = partners
					.Select(p => p.Code.Split('-').Last())
					.Select(n => int.Parse(n))
					.Max();

				nextNumber = maxNumber + 1;
			}

			return $"{prefix}-{nextNumber:D3}";
		}


		// ===== CREATE =====
		public async Task<PartnerCreateResponse> CreateAsync(CreatePartnerRequest request)
		{
			if (request.CommissionTableId.HasValue)
			{
				if (request.TableEndDate.HasValue && request.TableStartDate >= request.TableEndDate.Value)
				{
					throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc.");
				}

				var table = await _CommissiontableRepo.GetByIdAsync(request.CommissionTableId.Value);
				if (table == null) throw new AppException("Không tìm thấy Bảng giá này!", 404);
			}

			var code = await GeneratePartnerCode(request.CompanyName);

			var partner = new Partner
			{
				Id = Guid.NewGuid(),
				Code = code,
				PartnerTierId = request.PartnerTierId,
				CompanyName = request.CompanyName.Trim(),
				Address = request.Address.Trim(),
				Latitude = request.Latitude,
				Longitude = request.Longitude,
				IsActive = true,
				CreatedAt = _dateTime.UtcNow
			};

			await _partnerRepository.AddAsync(partner);
			CommissionTableApply? apply = null;
			if (request.CommissionTableId.HasValue)
			{
			    apply = new CommissionTableApply
				{
					Id = Guid.NewGuid(),
					PartnerId = partner.Id, 
					CommissionTableId = request.CommissionTableId.Value,
					Name = $"Áp dụng Bảng giá đợt đầu cho {partner.CompanyName}", 
					StartDate = request.TableStartDate ?? _dateTime.UtcNow,
					EndDate = request.TableEndDate,
					IsActive = true
				};

				await _commissionTableApplyRepo.AddAsync(apply);
			}

			await _unitOfWork.SaveChangesAsync();

			var createdPartner = await _partnerRepository.GetByIdWithTierAsync(partner.Id);

			return MapToCreateResponse(createdPartner!, apply);
		}


		// ================= GET =================
		public async Task<IEnumerable<PartnerResponse>> GetPartnersAsync(bool? isActive)
		{
			var stores = await _partnerRepository.GetPartnerAsync(isActive);
			return stores.Select(MapToResponse);
		}

		public async Task<PartnerDetailResponse> GetByIdAsync(Guid id)
		{
			var partner = await _partnerRepository.GetByIdWithTierAsync(id);

			if (partner == null)
				throw new AppException($"Partner not found. Id = {id}", 404);

			var mainUser = partner.Users?.FirstOrDefault();
			var mainAccount = mainUser?.Accounts?.FirstOrDefault(a =>
				a.AccountRoles.Any(ar => ar.Role.Name.ToLower().Trim() == "partneradmin"));

			return MapToDetailResponse(partner, mainUser, mainAccount);
		}

		// ===== UPDATE =====
		public async Task<PartnerResponse> UpdateAsync(Guid id, UpdatePartnerRequest request)
		{
			var partner = await _partnerRepository.GetByIdWithTierAsync(id);
			if (partner == null)
				throw new AppException($"Partner not found. Id = {id}", 404);

			partner.CompanyName = request.CompanyName.Trim();
			partner.Address = request.Address.Trim();
			partner.Latitude = request.Latitude;
			partner.Longitude = request.Longitude;
			partner.PartnerTierId = request.PartnerTierId;
			partner.UpdatedAt = _dateTime.UtcNow;

			_partnerRepository.Update(partner);
			await _unitOfWork.SaveChangesAsync();

			var updatedPartner = await _partnerRepository.GetByIdWithTierAsync(partner.Id);

			return MapToResponse(updatedPartner!);
		}

		// ===== DISABLE =====
		public async Task DisableAsync(Guid id)
		{
			var partner = await _partnerRepository.GetByIdAsync(id);
			if (partner == null)
				throw new KeyNotFoundException($"Partner not found. Id = {id}");

			if (!partner.IsActive)
				throw new InvalidOperationException("Partner is already inactive");

			partner.IsActive = false;
			partner.UpdatedAt = _dateTime.UtcNow;

			_partnerRepository.Update(partner);
			await _unitOfWork.SaveChangesAsync();
		}

		// ===== RESTORE =====
		public async Task RestoreAsync(Guid id)
		{
			var partner = await _partnerRepository.GetByIdAsync(id);
			if (partner == null)
				throw new KeyNotFoundException($"Partner not found. Id = {id}");

			if (partner.IsActive)
				throw new InvalidOperationException("Partner is already active");

			partner.IsActive = true;
			partner.UpdatedAt = _dateTime.UtcNow;

			_partnerRepository.Update(partner);
			await _unitOfWork.SaveChangesAsync();
		}

		// ===== DELETE (RARE – HARD DELETE) =====
		public async Task DeleteAsync(Guid id)
		{
			var partner = await _partnerRepository.GetByIdAsync(id);
			if (partner == null)
				throw new KeyNotFoundException($"Partner not found. Id = {id}");

			if (partner.Stores.Any())
				throw new InvalidOperationException("Cannot delete partner with existing stores");

			_partnerRepository.Remove(partner);
			await _unitOfWork.SaveChangesAsync();
		}

		// ===== MAPPER =====
		private static PartnerResponse MapToResponse(Partner partner)
		{
			return new PartnerResponse
			{
				Id = partner.Id,
				Code = partner.Code,
				CompanyName = partner.CompanyName,
				Address = partner.Address,
				Latitude = partner.Latitude,
				Longitude = partner.Longitude,

				PartnerTierId = partner.PartnerTierId,
				PartnerTierName = partner.PartnerTier?.Name ?? string.Empty,
				PartnerTierPriority = partner.PartnerTier?.Priority ?? 0,
				
				IsActive = partner.IsActive,

				CreatedAt = partner.CreatedAt,
				UpdatedAt = partner.UpdatedAt,

			};
		}

		private static PartnerDetailResponse MapToDetailResponse(Partner partner, User? mainUser, Account? mainAccount)
		{
			// 1. Xử lý danh sách Bảng hoa hồng (Nếu Repo có Include thì mới có data nhé sếp)
			var commissionApplies = partner.CommissionTableApplies?
				.OrderByDescending(c => c.StartDate) // Đưa cái mới nhất lên đầu
				.Select(c => new AppliedCommissionTableResponse
				{
					CommissionTableId = c.CommissionTableId,
					Name = c.CommissionTable?.Name ?? "Không rõ tên bảng giá", // Lấy tên từ bảng gốc
					StartDate = c.StartDate,
					EndDate = c.EndDate
				}).ToList() ?? new List<AppliedCommissionTableResponse>();

			// 2. Map dữ liệu trả về
			return new PartnerDetailResponse
			{
				// --- Thông tin Partner ---
				Id = partner.Id,
				Code = partner.Code,
				CompanyName = partner.CompanyName,
				Address = partner.Address,
				Latitude = partner.Latitude,
				Longitude = partner.Longitude,
				PartnerTierId = partner.PartnerTierId,
				PartnerTierName = partner.PartnerTier?.Name ?? string.Empty,
				PartnerTierPriority = partner.PartnerTier?.Priority ?? 0,
				IsActive = partner.IsActive,
				CreatedAt = partner.CreatedAt,
				UpdatedAt = partner.UpdatedAt,

				// --- Thông tin Account Admin ---
				PartnerAccount = mainUser != null ? new PartnerAdminResponse
				{
					Id = mainUser.Id,
					Email = mainUser.Email,
					FullName = mainUser.FullName,
					AvatarUrl = mainUser.AvatarUrl,
					IsActive = mainUser.IsActive,
					LastLoginAt = mainAccount?.LastLoginAt
				} : null,

				// 🚀 --- Bổ sung thông tin Commission cho Dashboard ---
				// Bảng đang áp dụng: Là bảng chưa có ngày kết thúc, hoặc ngày kết thúc vẫn còn ở tương lai
				CurrentCommission = commissionApplies.FirstOrDefault(c => c.EndDate == null || c.EndDate >= DateTime.UtcNow),

				// Toàn bộ lịch sử các bảng đã áp dụng
				CommissionHistories = commissionApplies
			};
		}

		private static PartnerCreateResponse MapToCreateResponse(Partner partner, Domain.Entities.CommissionTableApply? apply)
		{
			return new PartnerCreateResponse
			{
				Id = partner.Id,
				Code = partner.Code,
				CompanyName = partner.CompanyName,
				Address = partner.Address,
				Latitude = partner.Latitude,
				Longitude = partner.Longitude,
				PartnerTierId = partner.PartnerTierId,
				PartnerTierName = partner.PartnerTier?.Name ?? string.Empty,
				PartnerTierPriority = partner.PartnerTier?.Priority ?? 0,
				IsActive = partner.IsActive,
				CreatedAt = partner.CreatedAt,
				UpdatedAt = partner.UpdatedAt,

				// Ánh xạ bảng giá nếu có
				AppliedCommission = apply != null ? new AppliedCommissionTableResponse
				{
					CommissionTableId = apply.CommissionTableId,
					Name = apply.Name,
					StartDate = apply.StartDate,
					EndDate = apply.EndDate
				} : null
			};
		}
	}
}
