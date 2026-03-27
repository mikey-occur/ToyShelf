using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.User.Request
{
	public class GetWarehouseUsersRequest
	{
		public Guid? WarehouseId { get; set; }
		public WarehouseRole? Role { get; set; }
	}
}
