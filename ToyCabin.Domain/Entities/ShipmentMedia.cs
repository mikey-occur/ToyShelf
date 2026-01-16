using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class ShipmentMedia
	{
		public Guid Id { get; set; }
		public Guid ShipmentId { get; set; }
		public Guid UploadedByUserId { get; set; }
		public string MediaUrl { get; set; } = null!;
		public string MediaType { get; set; } = null!;   // IMAGE, VIDEO
		public string Purpose { get; set; } = null!;     // PICKUP, DELIVERY, DAMAGE

		public DateTime CreatedAt { get; set; }

		// Navigation
		public virtual Shipment Shipment { get; set; } = null!;
		public virtual User UploadedByUser { get; set; } = null!;
	}

}
