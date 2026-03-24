using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Inventory.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.InventoryLocation.Response
{
	public class InventoryLocationResponse
	{
		public Guid Id { get; set; }
		public InventoryLocationType Type { get; set; }
		public Guid? WarehouseId { get; set; }
		public Guid? StoreId { get; set; }
		public string Name { get; set; } = null!;
		public bool IsActive { get; set; }
	}
}
