using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shelf.Response
{
	public class ShelfDistributionResponse
	{
		public Guid InventoryLocationId { get; set; }
		public string InventoryLocationName { get; set; } = null!;
		public ShelfDetailResponse Shelf { get; set; } = null!;
		public int Quantity { get; set; }
		public ShelfStatus Status { get; set; }
	}
}
