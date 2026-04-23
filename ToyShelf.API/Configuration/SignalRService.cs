using Microsoft.AspNetCore.SignalR;
using ToyShelf.API.Hubs;
using ToyShelf.Application.IServices;

namespace ToyShelf.API.Configuration
{
    public class SignalRService : IProductBroadcaster, INotificationBroadcaster
	{
        private readonly IHubContext<ProductHub> _hubContext;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

		public SignalRService(IHubContext<ProductHub> hubContext, IHubContext<NotificationHub> notificationHubContext)
        {
            _hubContext = hubContext;
            _notificationHubContext = notificationHubContext;
		}
        public async Task NotifyProductSelectedAsync(Guid productId)
        {
            await _hubContext.Clients.All.SendAsync("OnProductSelected", productId);
        }

		public async Task SendNotificationToUserAsync(Guid userId, string title, string content)
		{
			string userRoom = $"noti:user:{userId}";
			await _notificationHubContext.Clients.Group(userRoom).SendAsync("ReceiveNewNotification", title, content);
		}

		public async Task SendNotificationToUserAsync(Guid userId, object payload)
		{
			string userRoom = $"noti:user:{userId}";

			await _notificationHubContext
				.Clients
				.Group(userRoom)
				.SendAsync("ReceiveNewNotification", payload);
		}
	}
}
