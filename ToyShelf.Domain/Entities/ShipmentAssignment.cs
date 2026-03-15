using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum AssignmentStatus
	{
		Pending,
		Accepted,
		Rejected
	}

	public class ShipmentAssignment
	{
		public Guid Id { get; set; }

		public Guid StoreOrderId { get; set; }

		public Guid ShipperId { get; set; }

		public AssignmentStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? RespondedAt { get; set; }

		public Guid AssignedByUserId { get; set; }

		public virtual StoreOrder StoreOrder { get; set; } = null!;
		public virtual User Shipper { get; set; } = null!;
		public virtual User AssignedByUser { get; set; } = null!;
		public virtual Shipment? Shipment { get; set; }
	}
}
