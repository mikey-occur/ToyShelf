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
		public DamageType Type { get; set; }
		public DamageSource Source { get; set; }
		public Guid? ProductColorId { get; set; }
		public Guid? ShelfId { get; set; }
		public int Quantity { get; set; }
		public string? Description { get; set; }
		public bool IsWarrantyClaim { get; set; }
		public List<string> MediaUrls { get; set; } = new List<string>();
	}
}
