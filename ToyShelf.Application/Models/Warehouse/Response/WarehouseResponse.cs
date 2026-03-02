using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Warehouse.Response
{
	public class WarehouseResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string? Address { get; set; }
		public bool IsActive { get; set; }

		public Guid CityId { get; set; }
		public string CityName { get; set; } = null!;
		public string CityCode { get; set; } = null!;

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
