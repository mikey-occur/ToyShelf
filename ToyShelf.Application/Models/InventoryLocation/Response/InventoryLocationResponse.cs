using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Inventory.Response;

namespace ToyShelf.Application.Models.InventoryLocation.Response
{
	public class InventoryLocationResponse
	{
		public Guid Id { get; set; }
		public string Type { get; set; } = null!;
		public Guid? WarehouseId { get; set; }
		public Guid? StoreId { get; set; }
		public string Name { get; set; } = null!;
		public bool IsActive { get; set; }
	}
}
