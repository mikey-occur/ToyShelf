using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShelfStatus
	{
		Available,
		InUse,
		Recalled,
		Maintenance,
		Retired
	}
	public class Shelf
	{
		public Guid Id { get; set; }
		public Guid? PartnerId { get; set; }
		public Guid? StoreId { get; set; }
		public string Code { get; set; } = null!;
		public int Level { get; set; }
		public ShelfStatus Status { get; set; }
		public DateTime? AssignedAt { get; set; }
		public DateTime? UnassignedAt { get; set; }
		public virtual Store? Store { get; set; }
		public virtual Partner? Partner { get; set; }
	}
}
