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

		public PartnerService(
			IPartnerRepository partnerRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_partnerRepository = partnerRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// ===== CREATE =====
		public async Task<PartnerResponse> CreateAsync(CreatePartnerRequest request)
		{
			var partner = new Partner
			{
				Id = Guid.NewGuid(),
				CompanyName = request.CompanyName.Trim(),
				Tier = request.Tier,
				RevenueSharePercent = request.RevenueSharePercent,
				IsActive = true,
				CreatedAt = _dateTime.UtcNow
			};

			await _partnerRepository.AddAsync(partner);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(partner);
		}

		// ================= GET =================
		public async Task<IEnumerable<PartnerResponse>> GetPartnersAsync(bool? isActive)
		{
			var stores = await _partnerRepository.GetPartnerAsync(isActive);
			return stores.Select(MapToResponse);
		}

		public async Task<PartnerResponse> GetByIdAsync(Guid id)
		{
			var partner = await _partnerRepository.GetByIdAsync(id);

			if (partner == null)
				throw new AppException($"Partner not found. Id = {id}", 404);

			return MapToResponse(partner);
		}

		// ===== UPDATE =====
		public async Task<PartnerResponse> UpdateAsync(Guid id, UpdatePartnerRequest request)
		{
			var partner = await _partnerRepository.GetByIdAsync(id);
			if (partner == null)
				throw new AppException($"Partner not found. Id = {id}", 404);

			partner.CompanyName = request.CompanyName.Trim();
			partner.Tier = request.Tier;
			partner.RevenueSharePercent = request.RevenueSharePercent;
			partner.UpdatedAt = _dateTime.UtcNow;

			_partnerRepository.Update(partner);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(partner);
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
				CompanyName = partner.CompanyName,
				Tier = partner.Tier,
				RevenueSharePercent = partner.RevenueSharePercent,
				IsActive = partner.IsActive,
				CreatedAt = partner.CreatedAt,
				UpdatedAt = partner.UpdatedAt
			};
		}
	}
}
