using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Warehouse.Request
{
	public class CreateWarehouseRequest
	{
		public Guid CityId { get; set; }
		public string? Code { get; set; }
		public string Name { get; set; } = null!;
		public string? Address { get; set; }
	}
}
