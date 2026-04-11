using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Request
{
	public class CreateShipmentRequest
	{
		public Guid ShipmentAssignmentId { get; set; }

		public List<ProductShipmentItemRequest> Products { get; set; } = new();
		public List<ShelfShipmentItemRequest> Shelves { get; set; } = new();
	}

	public class ProductShipmentItemRequest
	{
		public Guid StoreOrderId { get; set; }
		public Guid ProductColorId { get; set; }
		public int ExpectedQuantity { get; set; }
	}

	public class ShelfShipmentItemRequest
	{
		public Guid ShelfOrderId { get; set; }
		public Guid ShelfTypeId { get; set; }
		public int ExpectedQuantity { get; set; }
		public List<Guid> ShelfIds { get; set; } = new();
	}
}
