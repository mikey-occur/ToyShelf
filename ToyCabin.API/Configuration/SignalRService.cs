using Microsoft.AspNetCore.SignalR;
using ToyCabin.API.Hubs;
using ToyCabin.Application.IServices;

namespace ToyCabin.API.Configuration
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
