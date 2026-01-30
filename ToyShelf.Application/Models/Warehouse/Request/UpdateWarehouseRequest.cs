using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Warehouse.Request
{
	public class UpdateWarehouseRequest
	{
		public string Name { get; set; } = null!;
		public string? Address { get; set; }
	}
}
