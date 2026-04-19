using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.InventoryShelf.Response
{
	public class LocationShelvesResponse
	{
		public Guid LocationId { get; set; }
		public string LocationName { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;

		public List<ShelfGroupResponse> Shelves { get; set; } = new List<ShelfGroupResponse>();
	}
}
