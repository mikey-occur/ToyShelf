using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum CommissionTableType
	{
		Tier, // bảng giá thông thường
		Campaign, // Xả kho hàng
		Special // bảng giá độc quyền
	}

	public class CommissionTable
	{
		public Guid Id { get; set; }
		public Guid? PartnerTierId { get; set; }
		public string Name { get; set; } = null!;
		public CommissionTableType Type { get; set; }
		public bool IsActive { get; set; } = true;

		// Relationship ngược về Tier (để biết bảng này thường dùng cho Tier nào)
		public virtual PartnerTier? PartnerTier { get; set; }
		// Danh sách các mức % chi tiết
		public virtual ICollection<CommissionItem> CommissionItems { get; set; } = new List<CommissionItem>();
		//  áp dụng
		public virtual ICollection<CommissionTableApply> CommissionTableApplies { get; set; } = new List<CommissionTableApply>();

	}
}
