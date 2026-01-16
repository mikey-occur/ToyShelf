using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class ShipmentItem
	{
		public Guid Id { get; set; }
		public Guid ShipmentId { get; set; }
		public Guid ProductColorId { get; set; }

		public int ExpectedQuantity { get; set; }
		public int ReceivedQuantity { get; set; }

		public Shipment Shipment { get; set; } = null!;
		public ProductColor ProductColor { get; set; } = null!;
	}

}
