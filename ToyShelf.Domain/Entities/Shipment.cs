using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShipmentStatus
	{
		Draft,
		//Approved,
		Shipping,
		Delivered,
		Received,
		Cancelled
	}

	public class Shipment
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid FromLocationId { get; set; }
		public Guid ToLocationId { get; set; }

		public Guid ShipmentAssignmentId { get; set; }
		public Guid? ShipperId { get; set; }

		// Flag để phân biệt luồng: 
		// False: Warehouse -> Store (Giao) | True: Store -> Warehouse (Thu hồi)
		public bool IsReturn { get; set; }

		public Guid RequestedByUserId { get; set; }

		public ShipmentStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? PickedUpAt { get; set; }
		public DateTime? DeliveredAt { get; set; }
		public DateTime? ReceivedAt { get; set; }


		// Gom đơn
		public virtual ICollection<StoreOrder> StoreOrders { get; set; } = new List<StoreOrder>();
		public virtual ICollection<ShelfOrder> ShelfOrders { get; set; } = new List<ShelfOrder>();
		public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();

		public virtual InventoryLocation FromLocation { get; set; } = null!;
		public virtual InventoryLocation ToLocation { get; set; } = null!;
		public virtual ShipmentAssignment ShipmentAssignment { get; set; } = null!;
		public virtual User? Shipper { get; set; }
		public virtual User RequestedByUser { get; set; } = null!;

		// Items tổng hợp của tất cả các Order/Report trên
		public virtual ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
		public virtual ICollection<ShelfShipmentItem> ShelfShipmentItems { get; set; } = new List<ShelfShipmentItem>();
		public virtual ICollection<ShipmentMedia> Media { get; set; } = new List<ShipmentMedia>();

	}
}
