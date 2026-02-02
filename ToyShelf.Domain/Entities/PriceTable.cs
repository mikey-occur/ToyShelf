using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum PriceTableType
	{
		Tier,
		Clearance // Xả kho hàng
	}

	public class PriceTable
	{
		public Guid Id { get; set; }
		public Guid? PartnerTierId { get; set; }
		public string Name { get; set; } = null!;
		public PriceTableType Type { get; set; }
		public bool IsActive { get; set; } = true;

		// Relationship ngược về Tier (để biết bảng này thường dùng cho Tier nào)
		public virtual PartnerTier? PartnerTier { get; set; }
		// Danh sách các mức % chi tiết
		public virtual ICollection<PriceItem> PriceItems { get; set; } = new List<PriceItem>();
		// Lịch sử áp dụng
		public virtual ICollection<PriceTableApply> PriceTableApplies { get; set; } = new List<PriceTableApply>();

	}
}
