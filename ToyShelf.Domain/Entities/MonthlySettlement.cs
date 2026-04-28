using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class MonthlySettlement
	{
		public Guid Id { get; set; }

		// Của đối tác nào?
		public Guid PartnerId { get; set; }

		// Chốt cho kỳ nào?
		public int Month { get; set; }
		public int Year { get; set; }

		// Tổng hợp số liệu
		public int TotalItems { get; set; }
		public decimal TotalSalesAmount { get; set; }
		public decimal TotalCommissionAmount { get; set; }
		public decimal DeductionAmount { get; set; } = 0;
        public string? TransferReceiptUrl { get; set; }
        public string? Note { get; set; }
		public decimal FinalAmount { get; set; }

		// Trạng thái: PENDING (Chờ chuyển tiền), PAID (Đã chuyển khoản xong), 
		public string Status { get; set; } = "PENDING";


		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? PaidAt { get; set; } // Ngày giờ kế toán bấm nút "Đã chuyển tiền"

		public virtual Partner Partner { get; set; } = null!;
		public virtual ICollection<CommissionHistory> CommissionHistories { get; set; } = new List<CommissionHistory>();
	}
}
