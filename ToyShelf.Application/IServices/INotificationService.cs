using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Notification.Request;
using ToyShelf.Application.Models.Notification.Response;

namespace ToyShelf.Application.IServices
{
	public interface INotificationService
	{
		Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId);
		Task CreateNotificationAsync(CreateNotificationRequest request);
		Task CreateInternalNotificationAsync(InternalCreateNotificationRequest request);
		Task MarkAsReadAsync(Guid notificationId, Guid userId);
		Task CleanupOldNotificationsAsync(int daysToKeep = 30);
	}
}
