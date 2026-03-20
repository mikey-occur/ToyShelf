using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.ShipmentAssignment.Response
{
	public class ShipmentAssignmentResponse
	{
		public Guid Id { get; set; }

		public Guid StoreOrderId { get; set; }

		public string StoreOrderCode { get; set; } = null!;

		public Guid WarehouseLocationId { get; set; }

		public string WarehouseLocationName { get; set; } = null!;

		public Guid StoreLocationId { get; set; }

		public string StoreLocationName { get; set; } = null!;

		public string? ShipperName { get; set; }

		public string CreatedByName { get; set; } = null!;

		public string? AssignedByName { get; set; } 

		public AssignmentStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime? RespondedAt { get; set; }

		public List<ShipmentAssignmentItemResponse> Items { get; set; } = new();
	}
	public class ShipmentAssignmentItemResponse
	{
		public Guid ProductColorId { get; set; }
		public string ProductName { get; set; } = null!;

		public string Color { get; set; } = null!;

		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; } = 0;
	}

}
