using Microsoft.AspNetCore.SignalR;
using ToyShelf.API.Hubs;
using ToyShelf.Application.IServices;

namespace ToyShelf.API.Configuration
{
    public class SignalRService : IProductBroadcaster
    {
        private readonly IHubContext<ProductHub> _hubContext;
        
        public SignalRService(IHubContext<ProductHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task NotifyProductSelectedAsync(Guid productId)
        {
            await _hubContext.Clients.All.SendAsync("OnProductSelected", productId);
        }
    }
}
