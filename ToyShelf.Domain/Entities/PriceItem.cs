using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class PriceItem
	{
		public Guid Id { get; set; }

		// Thuộc bảng giá nào?
		public Guid PriceTableId { get; set; }

		// Áp dụng cho phân khúc nào?
		public Guid PriceSegmentId { get; set; }

		// Hoa hồng bao nhiêu %?
		public decimal CommissionRate { get; set; } // VD: 0.15 (15%)
		public virtual PriceTable PriceTable { get; set; } = null!;
		public virtual PriceSegment PriceSegment { get; set; } = null!;
	}
}
