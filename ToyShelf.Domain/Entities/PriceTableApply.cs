using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class PriceTableApply
	{
		public Guid Id { get; set; }

		// Áp dụng cho ai?
		public Guid PartnerId { get; set; }

		// Dùng bảng giá nào?
		public Guid PriceTableId { get; set; }

		// Hiệu lực từ ngày nào đến ngày nào?
		public string? Name { get; set; } // VD: "Ký phụ lục hợp đồng số 05/2024"
		
		public bool IsActive { get; set; } = true;

		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; } // Nếu null nghĩa là đang áp dụng vô thời hạn

		public virtual Partner Partner { get; set; } = null!;
		public virtual PriceTable PriceTable { get; set; } = null!;
	}
}
