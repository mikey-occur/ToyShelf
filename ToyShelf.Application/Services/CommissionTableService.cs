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
using static ToyShelf.Application.Models.PriceTable.Response.CommissionTableResponse;

namespace ToyShelf.Application.Services
{
	public class CommissionTableService : ICommissionTableService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommissionTableRepository _repo;
		public CommissionTableService(IUnitOfWork unitOfWork, ICommissionTableRepository repo)
		{
			_unitOfWork = unitOfWork;
			_repo = repo;
		}
		public async Task<CommissionTableResponse> CreateAsync(CommissionTableRequest request)
		{
			if (request.Type == CommissionTableType.Tier && !request.PartnerTierId.HasValue)
				throw new AppException("Partner tier is required for Tier Commission tables", 400);

			if (request.PartnerTierId.HasValue)
			{
				if (request.PartnerTierId.Value == Guid.Empty)
					throw new AppException("Partner tier is invalid", 400);

				var tierExists = await _unitOfWork.Repository<PartnerTier>()
					.AnyAsync(t => t.Id == request.PartnerTierId.Value);
				if (!tierExists)
					throw new AppException("Partner tier not found", 404);
			}

			if (request.Items != null && request.Items.Any())
			{
				var allCategoryIds = request.Items
					.Where(i => i.ProductCategoryIds != null)
					.SelectMany(i => i.ProductCategoryIds)
					.ToList();

				if (allCategoryIds.Count != allCategoryIds.Distinct().Count())
				{
					throw new AppException("Một danh mục sản phẩm không thể nằm ở nhiều mức hoa hồng khác nhau trong cùng một bảng giá!", 400);
				}
			}

			var commissionTable = new CommissionTable
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				PartnerTierId = request.PartnerTierId,
				Type = request.Type,
				CommissionItems = new List<CommissionItem>(),
				IsActive = true
			};

			if (request.Items != null && request.Items.Any())
			{
				foreach (var itemReq in request.Items)
				{
					var commissionItem = new CommissionItem
					{
						Id = Guid.NewGuid(),
						CommissionTableId = commissionTable.Id,
						CommissionRate = itemReq.CommissionRate,
						ItemCategories = new List<CommissionItemCategory>()
					};

					if (itemReq.ProductCategoryIds != null && itemReq.ProductCategoryIds.Any())
					{

						foreach (var categoryId in itemReq.ProductCategoryIds)
						{
							commissionItem.ItemCategories.Add(new CommissionItemCategory
							{
								CommissionItemId = commissionItem.Id,
								ProductCategoryId = categoryId
							});
						}
					}

					commissionTable.CommissionItems.Add(commissionItem);
				}
			}


			await _repo.AddAsync(commissionTable);
			await _unitOfWork.SaveChangesAsync();

			var fullTable = await _repo.GetByIdWithDetailsAsync(commissionTable.Id);
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
		public async Task<IEnumerable<CommissionTableResponse>> GetAllAsync()
		{
			var tables = await _repo.GetAllAsync();
			return tables.Select(MapToResponse);
		}

		public async Task<IEnumerable<CommissionTableResponse>> GetCommissionTablesAsync(bool? isActive)
		{
			var priceTables = await _repo.GetPriceTablesAsync(isActive);
			return priceTables.Select(MapToResponse);
		}

		public async Task<CommissionTableResponse> GetByIdAsync(Guid id)
		{
			var table = await _repo.GetByIdWithDetailsAsync(id)
				?? throw new AppException("Price Table not found", 404);
			return MapToResponse(table);
		}

		public async Task<CommissionTableResponse> UpdateAsync(Guid id, CommissionTableUpdateRequest request)
		{
			var commissionTable = await _repo.GetByIdWithDetailsAsync(id)
		  ?? throw new AppException("Price Table not found", 404);

			if (request.Type == CommissionTableType.Tier && !request.PartnerTierId.HasValue)
				throw new AppException("Partner tier is required for Tier price tables", 400);

			if (request.PartnerTierId.HasValue)
			{
				if (request.PartnerTierId.Value == Guid.Empty)
					throw new AppException("Partner tier is invalid", 400);

				var tierExists = await _unitOfWork.Repository<PartnerTier>()
					.AnyAsync(t => t.Id == request.PartnerTierId.Value);
				if (!tierExists)
					throw new AppException("Partner tier not found", 404);
			}

			if (request.Items != null && request.Items.Any())
			{
				var allCategoryIds = request.Items
					.Where(i => i.ProductCategoryIds != null)
					.SelectMany(i => i.ProductCategoryIds)
					.ToList();

				if (allCategoryIds.Count != allCategoryIds.Distinct().Count())
				{
					throw new AppException("Một danh mục sản phẩm không thể nằm ở nhiều mức hoa hồng khác nhau trong cùng một bảng giá!", 400);
				}
			}


			// Cập nhật thông tin cha
			commissionTable.Name = request.Name;
			commissionTable.PartnerTierId = request.PartnerTierId;
			commissionTable.Type = request.Type;
			commissionTable.IsActive = request.IsActive;


			// ===== Xử lý CommissionItems =====
			if (request.Items != null)
			{
				// Xoá toàn bộ item cũ
				if (commissionTable.CommissionItems != null && commissionTable.CommissionItems.Any())
				{
					_unitOfWork.Repository<CommissionItem>()
						.DeleteRange(commissionTable.CommissionItems);
				}

				// Reset lại list Items để chuẩn bị hứng đồ mới
				// Thêm item mới
				foreach (var itemReq in request.Items)
				{
					var newItem = new CommissionItem
					{
						Id = Guid.NewGuid(),
						CommissionTableId = commissionTable.Id,
						CommissionRate = itemReq.CommissionRate,
						ItemCategories = new List<CommissionItemCategory>() 
					};

					if (itemReq.ProductCategoryIds != null && itemReq.ProductCategoryIds.Any())
					{
						foreach (var categoryId in itemReq.ProductCategoryIds)
						{
							newItem.ItemCategories.Add(new CommissionItemCategory
							{
								CommissionItemId = newItem.Id,
								ProductCategoryId = categoryId
							});
						}
					}

					await _unitOfWork.Repository<CommissionItem>().AddAsync(newItem);
				}
			}

			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(commissionTable);
		}

		// Mapping 
		private static CommissionTableResponse MapToResponse(CommissionTable entity)
		{
			return new CommissionTableResponse
			{
				Id = entity.Id,
				Name = entity.Name,
				Type = entity.Type.ToString(),
				PartnerTierId = entity.PartnerTierId,
				PartnerTierName = entity.PartnerTier?.Name,
				IsActive = entity.IsActive,
				Items = entity.CommissionItems.Select(i => new CommissionItemResponse
				{
					Id = i.Id,
					CommissionRate = i.CommissionRate,
					AppliedCategories = i.ItemCategories
					.Where(ic => ic.ProductCategory != null) 
					.Select(ic => new AppliedCategoryResponse
					{
						Id = ic.ProductCategory.Id,
						Name = ic.ProductCategory.Name,
						Code = ic.ProductCategory.Code
					}).ToList()

				}).ToList()
			};
		}

	}
}
