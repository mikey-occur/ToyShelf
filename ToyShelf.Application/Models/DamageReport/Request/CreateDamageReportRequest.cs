using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.DamageReport.Request
{
	public class CreateDamageReportRequest
	{
		public DamageSource Source { get; set; }
		public string? Description { get; set; }
		public bool IsWarrantyClaim { get; set; }

		// Danh sách các món hỏng hóc
		public List<CreateDamageItemRequest> Items { get; set; } = new List<CreateDamageItemRequest>();
	}

	public class CreateDamageItemRequest
	{
		public DamageItemType Type { get; set; } // Product | Shelf
		public Guid? ProductColorId { get; set; }
		public Guid? ShelfId { get; set; }
		public int Quantity { get; set; }

		// Media gắn riêng cho từng món
		public List<string> MediaUrls { get; set; } = new List<string>();
	}
}
