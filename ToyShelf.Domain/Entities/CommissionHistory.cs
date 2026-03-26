using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class CommissionHistory
	{
		public Guid Id { get; set; }
		public Guid OrderItemId { get; set; }
		public Guid PartnerId { get; set; }
		public Guid? MonthlySettlementId { get; set; }
		public decimal SalesAmount { get; set; }  // Giá bán của món đồ chơi lúc đó (VD: 100.000)
		public decimal AppliedRate { get; set; }  // % hoa hồng lúc tính (VD: 0.1)
		public decimal CommissionAmount { get; set; }  
		public DateTime CreatedAt { get; set; }
		public virtual MonthlySettlement? MonthlySettlement { get; set; }
		public virtual Partner Partner { get; set; } = null!;
		public virtual OrderItem OrderItem { get; set; } = null!;


	}
}
