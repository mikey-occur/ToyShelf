using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Notification.Request
{
	public class InternalCreateNotificationRequest
	{
		public Guid UserId { get; set; }
		public string Title { get; set; } = "Thông báo";
		public string Content { get; set; } = null!;

		public string? RefType { get; set; }
		public Guid? RefId { get; set; }
	}
}
