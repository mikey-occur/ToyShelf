using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{

	public enum WarehouseRole
	{
		Manager,
		Shipper
	}
	public class UserWarehouse
	{
		public Guid Id { get; set; }

		public Guid UserId { get; set; }
		public Guid WarehouseId { get; set; }

		public WarehouseRole Role { get; set; } // Manager, Shipper
		public bool IsActive { get; set; } = true;

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		public virtual User User { get; set; } = null!;
		public virtual Warehouse Warehouse { get; set; } = null!;
	}

}
