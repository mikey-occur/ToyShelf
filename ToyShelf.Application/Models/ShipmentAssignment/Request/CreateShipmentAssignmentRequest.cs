using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ShipmentAssignment.Request
{
	public class CreateShipmentAssignmentRequest
	{
		public Guid WarehouseLocationId { get; set; }
		public Guid? StoreOrderId { get; set; }
		public Guid? ShelfOrderId { get; set; }

		public List<ItemAllocationRequest>? StoreItems { get; set; }
		public List<ItemAllocationRequest>? ShelfItems { get; set; }
	}

	public class ItemAllocationRequest
	{
		public Guid ItemId { get; set; } 
		public int Quantity { get; set; } 
	}
}
