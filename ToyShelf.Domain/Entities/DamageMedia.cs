using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class DamageMedia
	{
		public Guid Id { get; set; }
		public Guid DamageReportItemId { get; set; }
		public string MediaUrl { get; set; } = null!;
		public string MediaType { get; set; } = null!;   // IMAGE, VIDEO
		public DateTime CreatedAt { get; set; }

		// Navigation
		public virtual DamageReportItem DamageReportItem { get; set; } = null!;
	}
}
