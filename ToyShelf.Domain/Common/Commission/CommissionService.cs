using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Domain.Common.Commission
{
	public class CommissionService : ICommissionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommissionItemRepository _commissionItemRepository;
		private readonly IProductColorRepository _productColorRepository;
		private readonly ICommissionTableApplyRepository _commissionTableApplyRepository;

		private readonly IPartnerRepository _partnerRepository;
		public CommissionService(
			IUnitOfWork unitOfWork,
			ICommissionItemRepository CommisisonItemRepository,
			IProductColorRepository productColorRepository,
			ICommissionTableApplyRepository commissionTableApplyRepository,
			IPartnerRepository partnerRepository)
		{
			_unitOfWork = unitOfWork;
			_commissionItemRepository = CommisisonItemRepository;
			_productColorRepository = productColorRepository;
			_commissionTableApplyRepository = commissionTableApplyRepository;
			_partnerRepository = partnerRepository;
		}

		public async Task<CommissionCalculationResult> CalculateCommissionAsync(Guid partnerId, Guid productColorId, decimal soldPrice)
		{
			
			// 1. LẤY THÔNG TIN SẢN PHẨM VÀ DANH MỤC TRỰC TIẾP CỦA SẢN PHẨM
			var productColor = await _productColorRepository.GetByIdWithProductAsync(productColorId);
			if (productColor == null || productColor.Product == null)
			{
			
				return new CommissionCalculationResult(0, "Không tìm thấy sản phẩm");
			}

			// 🚀 Lấy thẳng 1 Category ID duy nhất của sản phẩm
			var categoryId = productColor.Product.ProductCategoryId;

			// 2. LẤY CÁC BẢNG GIÁ ĐANG ÁP DỤNG CỦA ĐỐI TÁC VÀ XẾP HẠNG ƯU TIÊN
			var currentTime = DateTime.UtcNow;
			
			var activeApplies = await _commissionTableApplyRepository.GetActiveAppliesByPartnerAsync(partnerId, currentTime);

			if (!activeApplies.Any())
			{
				
				return new CommissionCalculationResult(0, "Đối tác không có bảng giá nào đang hoạt động");
			}

		
			// Xếp hạng: Special (Ưu tiên 1) -> Campaign (Ưu tiên 2) -> Tier (Ưu tiên 3)
			var sortedApplies = activeApplies.OrderBy(a => a.CommissionTable.Type).ToList();

			// 3. THUẬT TOÁN QUÉT TÌM % HOA HỒNG THEO BẢNG ƯU TIÊN
			foreach (var apply in sortedApplies)
			{
				var table = apply.CommissionTable;
				if (table.CommissionItems == null || !table.CommissionItems.Any()) continue;

				
				var matchedItem = table.CommissionItems.FirstOrDefault(i =>
					i.ItemCategories != null &&
					i.ItemCategories.Any(ic => ic.ProductCategoryId == categoryId));

				if (matchedItem != null)
				{
				
					return new CommissionCalculationResult(
						matchedItem.CommissionRate,
						$"Bảng [{table.Type}] {table.Name} - Cấu hình trực tiếp cho danh mục ID: {categoryId}"
					);
				}
			}
		
			// Nếu quét qua các bảng (Special -> Campaign -> Tier) mà không cái nào cấu hình cho Category này
			return new CommissionCalculationResult(0, "Sản phẩm không nằm trong danh mục được chiết khấu của các bảng giá hiện tại");
		}
	}
}

