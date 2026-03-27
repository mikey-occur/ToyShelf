using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.User.Response
{
	public class WarehouseDetailByUserResponse
	{
		public Guid UserId { get; set; }
		public string Email { get; set; } = null!;
		public string FullName { get; set; } = null!;

		public Guid? WarehouseId { get; set; }
		public List<Guid> WarehouseLocationIds { get; set; } = new();
		public string? WarehouseName { get; set; }
		public WarehouseRole? WarehouseRole { get; set; }
	}
}
