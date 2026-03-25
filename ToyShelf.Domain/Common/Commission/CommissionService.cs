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
		private readonly ICommissionPolicyRepository _commissionPolicyRepository;
		private readonly IPartnerRepository _partnerRepository;
		public CommissionService(
			IUnitOfWork unitOfWork,
			ICommissionItemRepository CommisisonItemRepository,
			IProductColorRepository productColorRepository,
			ICommissionTableApplyRepository commissionTableApplyRepository,
			ICommissionPolicyRepository commissionPolicyRepository,
			IPartnerRepository partnerRepository)
		{
			_unitOfWork = unitOfWork;
			_commissionItemRepository = CommisisonItemRepository;
			_productColorRepository = productColorRepository;
			_commissionTableApplyRepository = commissionTableApplyRepository;
			_commissionPolicyRepository = commissionPolicyRepository;
			_partnerRepository = partnerRepository;
		}

		public async Task<CommissionCalculationResult> CalculateCommissionAsync(Guid partnerId, Guid productColorId, decimal soldPrice)
		{
			Console.WriteLine($"\n[DEBUG-1] Bắt đầu tính tiền cho Partner: {partnerId} | ProductColor: {productColorId}");
			// 1. LẤY THÔNG TIN SẢN PHẨM VÀ DANH MỤC TRỰC TIẾP CỦA SẢN PHẨM
			var productColor = await _productColorRepository.GetByIdWithProductAsync(productColorId);
			if (productColor == null || productColor.Product == null)
			{
				Console.WriteLine("[DEBUG-2] LỖI: Không lấy được Product hoặc Category! (Hãy check lại hàm GetByIdWithProductAsync xem đã Include Product chưa)");
				return new CommissionCalculationResult(0, "Không tìm thấy sản phẩm");
			}

			// 🚀 Lấy thẳng 1 Category ID duy nhất của sản phẩm
			var categoryId = productColor.Product.ProductCategoryId;

			// 2. LẤY CÁC BẢNG GIÁ ĐANG ÁP DỤNG CỦA ĐỐI TÁC VÀ XẾP HẠNG ƯU TIÊN
			var currentTime = DateTime.UtcNow;
			Console.WriteLine($"[DEBUG-4] Đang tìm bảng giá bằng thời gian (UTC): {currentTime:yyyy-MM-dd HH:mm:ss}");
			var activeApplies = await _commissionTableApplyRepository.GetActiveAppliesByPartnerAsync(partnerId, currentTime);

			if (!activeApplies.Any())
			{
				Console.WriteLine("[DEBUG-5] LỖI: Không tìm thấy Bảng giá nào Active. Lý do có thể là: Chưa Apply, Bị tắt IsActive, hoặc MÚI GIỜ BỊ LỆCH (Giờ UTC nhỏ hơn StartDate trong DB)");
				return new CommissionCalculationResult(0, "Đối tác không có bảng giá nào đang hoạt động");
			}

			Console.WriteLine($"[DEBUG-6] Tìm thấy {activeApplies.Count} bảng giá. Bắt đầu lội tìm Category...");
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
					Console.WriteLine($"[DEBUG-7] THÀNH CÔNG! Chốt % ở bảng [{table.Type}], Mức %: {matchedItem.CommissionRate}");
					return new CommissionCalculationResult(
						matchedItem.CommissionRate,
						$"Bảng [{table.Type}] {table.Name} - Cấu hình trực tiếp cho danh mục ID: {categoryId}"
					);
				}
			}
			Console.WriteLine("[DEBUG-8] LỖI: Tìm thấy bảng giá nhưng KHÔNG CÓ CẤU HÌNH % NÀO KHỚP với Category ID của sản phẩm này!");
			// Nếu quét qua các bảng (Special -> Campaign -> Tier) mà không cái nào cấu hình cho Category này
			return new CommissionCalculationResult(0, "Sản phẩm không nằm trong danh mục được chiết khấu của các bảng giá hiện tại");
		}
	}
}

