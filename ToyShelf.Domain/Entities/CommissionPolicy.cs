using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class CommissionPolicy
	{
		public Guid Id { get; set; }
		// Khóa ngoại trỏ về Tier (Đồng, Bạc...)
		public Guid PartnerTierId { get; set; }
		// Khóa ngoại trỏ về Segment (Rẻ, Trung, Mắc...)
		public Guid PriceSegmentId { get; set; }
		// Mức hoa hồng (VD: 0.10 cho 10%, 0.15 cho 15%)
		public decimal CommissionRate { get; set; }
		// (Tùy chọn) Ngày áp dụng để quản lý lịch sử thay đổi chính sách

		public DateTime? EffectiveDate { get; set; }
		public virtual PriceSegment PriceSegment { get; set; } = null!;
		public virtual PartnerTier PartnerTier { get; set; } = null!;
	}
}
