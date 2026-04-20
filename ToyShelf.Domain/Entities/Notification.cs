using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Notification
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Title { get; set; } = null!;
		public string Content { get; set; } = null!;
		public string? RefType { get; set; }
		public Guid? RefId { get; set; }
		public bool IsRead { get; set; } = false;
		public DateTime CreatedAt { get; set; }
		public DateTime? ReadAt { get; set; }
		public virtual User User { get; set; } = null!;
	}
}
