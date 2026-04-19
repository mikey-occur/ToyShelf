using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.InventoryShelf.Response
{
	public class InventoryShelfResponse
	{
		public Guid InventoryLocationId { get; set; }
		public string LocationName { get; set; } = null!;

		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = null!;
		public string? ImageUrl { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public double Depth { get; set; }
    	public string? DisplayGuideline { get; set; }
    	public int TotalLevels { get; set; }
		public int Quantity { get; set; }
		public ShelfStatus Status { get; set; }

	}
}
