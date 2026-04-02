using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShipmentShelfItemResponse
	{
		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = null!;
		public string? ImageUrl { get; set; }
		public int ExpectedQuantity { get; set; }
		public int ReceivedQuantity { get; set; }
	}
}
