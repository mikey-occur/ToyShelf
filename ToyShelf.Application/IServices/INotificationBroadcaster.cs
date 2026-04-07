using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.IServices
{
	public interface INotificationBroadcaster
	{
		Task SendNotificationToUserAsync(Guid userId, string title, string content);
	}
}
