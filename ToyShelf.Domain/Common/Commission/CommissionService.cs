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
		private readonly ICommissionItemRepository _priceItemRepository;
		private readonly IProductColorRepository _productColorRepository;
		private readonly ICommissionTableApplyRepository _priceTableApplyRepository;
		private readonly ICommissionPolicyRepository _commissionPolicyRepository;
		private readonly IPartnerRepository _partnerRepository;
		public CommissionService(
			IUnitOfWork unitOfWork,
			ICommissionItemRepository priceItemRepository,
			IProductColorRepository productColorRepository,
			ICommissionTableApplyRepository priceTableApplyRepository,
			ICommissionPolicyRepository commissionPolicyRepository,
			IPartnerRepository partnerRepository)
		{
			_unitOfWork = unitOfWork;
			_priceItemRepository = priceItemRepository;
			_productColorRepository = productColorRepository;
			_priceTableApplyRepository = priceTableApplyRepository;
			_commissionPolicyRepository = commissionPolicyRepository;
			_partnerRepository = partnerRepository;
		}

		public async Task<CommissionCalculationResult> CalculateCommissionAsync(Guid partnerId, Guid productColorId, decimal soldPrice)
		{
			// Lay phan khuc gia
			var productColor = await _productColorRepository.GetByIdWithSegmentAsync(productColorId);
			if (productColor == null || productColor.PriceSegment == null)
			{
				return new CommissionCalculationResult(0, "product not found");
			}
			var segment = productColor.PriceSegment;

			// get price table apply
			//var activeApply = await _priceTableApplyRepository.GetActiveByPartnerAsync(partnerId, DateTime.UtcNow);
			//if (activeApply != null)
			//{
			//	var priceItem = await   _priceItemRepository.GetItemAsync(activeApply.CommissionTableId, segment.Id);
			//	if (priceItem != null)
			//	{
			//		return new CommissionCalculationResult(
			//			priceItem.CommissionRate,
			//			$"Bảng giá của partner: {activeApply.CommissionTable.Name} ({segment.Name})");
			//	}
			//}

			// 3. ƯU TIÊN 2: Dùng chính sách mặc định theo Tier của Partner
			var partner = await _partnerRepository.GetByIdAsync(partnerId);
			if (partner != null)
			{
				var policy = await _commissionPolicyRepository.GetPolicyAsync(partner.PartnerTierId, segment.Id);
				if (policy != null)
				{
					return new CommissionCalculationResult(
						policy.CommissionRate,
						$"Chính sách mặc định (Tier)");
				}
			}
			return new CommissionCalculationResult(0, "Không tìm thấy chính sách áp dụng");
		}
	}
}
