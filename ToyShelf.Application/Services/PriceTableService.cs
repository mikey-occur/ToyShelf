using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceTable.Request;
using ToyShelf.Application.Models.PriceTable.Response;
using ToyShelf.Application.Models.ProductColor.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using static ToyShelf.Application.Models.PriceTable.Response.PriceTableResponse;

namespace ToyShelf.Application.Services
{
	public class PriceTableService : IPriceTableService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPriceTableRepository _repo;
		public PriceTableService(IUnitOfWork unitOfWork, IPriceTableRepository repo)
		{
			_unitOfWork = unitOfWork;
			_repo = repo;
		}
		public async Task<PriceTableResponse> CreateAsync(PriceTableRequest request)
		{
			
			var priceTable = new PriceTable
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				PartnerTierId = request.PartnerTierId,
				Type = request.Type,
				IsActive = true
			};

			if (request.Items != null && request.Items.Any())
			{
				foreach (var itemReq in request.Items)
				{
					var priceItem = new PriceItem
					{
						Id = Guid.NewGuid(),
						PriceTableId = priceTable.Id, 
						PriceSegmentId = itemReq.PriceSegmentId,
						CommissionRate = itemReq.CommissionRate
					};
				
					priceTable.PriceItems.Add(priceItem);
				}
			}

			
			await _repo.AddAsync(priceTable);
			await _unitOfWork.SaveChangesAsync();

			var fullTable = await _repo.GetByIdWithDetailsAsync(priceTable.Id);
			return MapToResponse(fullTable!);
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var priceTable = await _repo.GetByIdAsync(id)
			?? throw new KeyNotFoundException("Price Table not found");

			
		   bool isInUse = await _repo.IsPriceTableInUseAsync(id);
			if (isInUse)
			{
				throw new InvalidOperationException("Can't not delete because in use");
			}


			_repo.Remove(priceTable);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		public async Task<bool> RestorePriceTableAsync(Guid id)
		{
			var priceTable = await _repo.GetByIdAsync(id);
			if (priceTable == null)
				throw new AppException("Price Table not found", 404);
			if (priceTable.IsActive)
				throw new AppException("Price Table already active", 400);
			priceTable.IsActive = true;
			_repo.Update(priceTable);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}
	
		public async Task<bool> DisablePriceTableAsync(Guid id)
		{
			var priceTable = await _repo.GetByIdAsync(id);
			if (priceTable == null)
				throw new AppException("Price Table not found", 404);
			if (!priceTable.IsActive)
				throw new AppException("Price Table already inactive", 400);
			priceTable.IsActive = false;
			_repo.Update(priceTable);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}
		public async Task<IEnumerable<PriceTableResponse>> GetAllAsync()
		{
			var tables = await _repo.GetAllAsync();
			return tables.Select(MapToResponse);
		}

		public async Task<IEnumerable<PriceTableResponse>> GetPriceTablesAsync(bool? isActive)
		{
			var priceTables = await _repo.GetPriceTablesAsync(isActive);
			return priceTables.Select(MapToResponse);
		}

		public async Task<PriceTableResponse> GetByIdAsync(Guid id)
		{
			var table = await _repo.GetByIdWithDetailsAsync(id)
				?? throw new KeyNotFoundException("Price Table not found");
			return MapToResponse(table);
		}

		public async Task<PriceTableResponse> UpdateAsync(Guid id, PriceTableUpdateRequest request)
		{
			var priceTable = await _repo.GetByIdWithDetailsAsync(id)
			?? throw new KeyNotFoundException("Price Table not found");

			priceTable.Name = request.Name;
			priceTable.PartnerTierId = request.PartnerTierId;
			priceTable.Type = request.Type;
			priceTable.IsActive = request.IsActive;

			priceTable.PriceItems.Clear();

			if (request.Items != null)
			{
				foreach (var itemReq in request.Items)
				{
					priceTable.PriceItems.Add(new PriceItem
					{
						Id = Guid.NewGuid(),
						PriceTableId = priceTable.Id,
						PriceSegmentId = itemReq.PriceSegmentId,
						CommissionRate = itemReq.CommissionRate
					});
				}
			}

			_repo.Update(priceTable);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(priceTable);
		}

		// Mapping 
		private static PriceTableResponse MapToResponse(PriceTable entity)
		{
			return new PriceTableResponse
			{
				Id = entity.Id,
				Name = entity.Name,
				Type = entity.Type.ToString(),
				PartnerTierId = entity.PartnerTierId,
				PartnerTierName = entity.PartnerTier?.Name,
				IsActive = entity.IsActive,
				Items = entity.PriceItems.Select(i => new PriceItemResponse
				{
					Id = i.Id,
					PriceSegmentId = i.PriceSegmentId,
					PriceSegmentName = i.PriceSegment?.Name ?? "Unknown",
					CommissionRate = i.CommissionRate
				}).ToList()
			};
		}

	}
}
