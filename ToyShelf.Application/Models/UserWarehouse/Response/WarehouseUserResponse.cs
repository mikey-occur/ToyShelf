using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.UserWarehouse.Response
{
	public class WarehouseUserResponse
	{
		public Guid UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string? AvatarUrl { get; set; }
		public bool IsActive { get; set; } = true;
		public string Role { get; set; } = string.Empty;
	}
}
