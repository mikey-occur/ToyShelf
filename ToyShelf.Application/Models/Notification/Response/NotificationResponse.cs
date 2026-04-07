using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Notification.Response
{
	public class NotificationResponse
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public bool IsRead { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
