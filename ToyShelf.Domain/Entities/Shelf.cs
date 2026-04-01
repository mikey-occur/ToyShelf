using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShelfStatus
	{
		Available,     // đang nằm ở warehouse
		InUse,         // đang ở store và sử dụng
		Recalled,      // bị thu hồi
		Maintenance,   // bảo trì
		Retired        // bỏ
	}
	public class Shelf
	{
		public Guid Id { get; set; }
		public Guid InventoryLocationId { get; set; }
		public Guid ShelfTypeId { get; set; }
		public string Code { get; set; } = null!;
		public ShelfStatus Status { get; set; }
		public DateTime? AssignedAt { get; set; }
		public DateTime? UnassignedAt { get; set; }
		public virtual ShelfType ShelfType { get; set; } = null!;
		public virtual InventoryLocation InventoryLocation { get; set; } = null!;
	}
}
