using Microsoft.AspNetCore.SignalR;

namespace ToyCabin.API.Hubs
{
    public class ProductHub : Hub
    {
        public async Task SelectProduct(string productId)
        {
            // Gửi cho tất cả client khác (trừ người gửi)
            await Clients.Others.SendAsync("OnProductSelected", productId);
        }
    }
}
