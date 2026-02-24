using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Warehouse
	{
		public Guid Id { get; set; }
		public Guid CityId { get; set; }
		public string Code { get; set; } = null!;   // CENTRAL-HCM
		public string Name { get; set; } = null!;
		public string? Address { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		// Navigation
		public virtual ICollection<InventoryLocation> InventoryLocations { get; set; } = new List<InventoryLocation>();
		public virtual City City { get; set; } = null!;
	}
}
