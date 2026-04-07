using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Notification.Request
{
	public class CreateNotificationRequest
	{
		public Guid UserId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
	}
}
