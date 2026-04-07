using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.DamageReport.Response
{
	public class DamageReportResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = string.Empty;
		public DamageType Type { get; set; }
		public DamageSource Source { get; set; }
		public DamageStatus Status { get; set; }

		// Thông tin sản phẩm (nếu hỏng hàng)
		public Guid? ProductColorId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string SKU { get; set; } = string.Empty;
		public string ColorName { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }

		// Thông tin kệ (nếu hỏng kệ/thiết bị IoT)
		public Guid? ShelfId { get; set; }
		public string ShelfCode { get; set; } = string.Empty;

		public int Quantity { get; set; }
		public string? Description { get; set; }
		public string? AdminNote { get; set; }
		public bool IsWarrantyClaim { get; set; }

		public Guid ReportedByUserId { get; set; }
		public string ReportedByName { get; set; } = string.Empty;
		public Guid? ReviewedByUserId { get; set; }
		public string ReviewedByName { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; }
		public DateTime? ReviewedAt { get; set; }
		public List<string> MediaUrls { get; set; } = new List<string>();
	}
}
