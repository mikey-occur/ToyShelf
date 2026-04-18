using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.DamageReport.Response
{
	public class DamageReturnItemResponse
	{
		public Guid DamageReportItemId { get; set; }
		public Guid DamageReportId { get; set; }
		public string DamageCode { get; set; } = null!;
		public string DamageType { get; set; } = null!; // "Product" hoặc "Shelf"
		public int Quantity { get; set; }
		public string TargetName { get; set; } = null!; // Tên SP hoặc Mã Kệ (SH-0001)
		public string? ImageUrl { get; set; }
	}
}
