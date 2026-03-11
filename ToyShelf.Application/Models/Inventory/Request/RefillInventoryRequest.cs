using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Inventory.Request
{
	public class RefillInventoryRequest
	{
		public Guid InventoryLocationId { get; set; }
		public Guid ProductColorId { get; set; }
		public int Quantity { get; set; }
	}
}
