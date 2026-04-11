using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShipmentMediaPurpose
	{
		Pickup,
		Delivery,
		Damage,
		ReturnPickup
	}

	public enum ShipmentMediaType
	{
		Image,
		Video
	}
	public class ShipmentMedia
	{
		public Guid Id { get; set; }
		public Guid ShipmentId { get; set; }
		public Guid UploadedByUserId { get; set; }
		public string MediaUrl { get; set; } = null!;
		public ShipmentMediaType MediaType { get; set; } // IMAGE, VIDEO
		public ShipmentMediaPurpose Purpose { get; set; } // PICKUP, DELIVERY, DAMAGE

		public DateTime CreatedAt { get; set; }

		// Navigation
		public virtual Shipment Shipment { get; set; } = null!;
		public virtual User UploadedByUser { get; set; } = null!;
	}
}
