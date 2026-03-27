using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.UserWarehouse.Request
{
	public class CreateWarehouseUserRequest
	{
		public string Email { get; set; } = null!;
		public string FullName { get; set; } = null!;

		public Guid WarehouseId { get; set; }
		public WarehouseRole Role { get; set; } // Manager | Shipper
	}
}
