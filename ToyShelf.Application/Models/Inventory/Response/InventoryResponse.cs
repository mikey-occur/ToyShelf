using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Inventory.Response
{
	public class InventoryResponse
	{
		public Guid Id { get; set; }
		public Guid InventoryLocationId { get; set; }
		public Guid ProductColorId { get; set; }
		public Guid DispositionId { get; set; }
		public int Quantity { get; set; }
	}

}
