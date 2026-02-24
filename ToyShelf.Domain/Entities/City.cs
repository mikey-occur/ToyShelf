using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class City
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual ICollection<Warehouse> Warehouses { get; set; }
	= new List<Warehouse>();
	}
}
