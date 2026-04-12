using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.InventoryShelf.Response
{
	public class InventoryShelfResponse
	{
		public Guid InventoryLocationId { get; set; }
		public string LocationName { get; set; } = null!;

		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = null!;

		public int Quantity { get; set; }

		public int TotalLevels { get; set; }
	}
}
