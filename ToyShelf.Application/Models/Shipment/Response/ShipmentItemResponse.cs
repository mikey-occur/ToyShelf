using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShipmentItemResponse
	{
		public Guid ProductColorId { get; set; }
		public string SKU { get; set; } = string.Empty;
		public string ProductName { get; set; } = null!;
		public string Color { get; set; } = null!;
		public string? ImageUrl { get; set; }
		public int ExpectedQuantity { get; set; }
		public int ReceivedQuantity { get; set; }
	}
}
