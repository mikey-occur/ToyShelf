using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Cabin
	{
		public Guid Id { get; set; }
		public Guid? StoreId { get; set; }
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string? LocationDescription { get; set; }
		public bool IsOnline { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? LastHeartbeatAt { get; set; }
		public virtual Store? Store { get; set; }
		public virtual ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
		public virtual ICollection<CabinSnapshot> CabinSnapshots { get; set; } = new List<CabinSnapshot>();
	}
}
