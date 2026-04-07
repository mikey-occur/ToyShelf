using Microsoft.AspNetCore.SignalR;

namespace ToyShelf.API.Hubs
{
	public class NotificationHub : Hub
	{
		public async Task JoinSystem(string userId, string role)
		{
			// 1. Nhét User vào phòng CÁ NHÂN (Để nhận thông báo riêng 1-1. VD: Có đơn hàng mới)
			string userRoom = $"noti:user:{userId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, userRoom);

			// 2. Nhét User vào phòng CHUNG theo Role (Để nhận thông báo hàng loạt. VD: Bảng giá mới cho Partner)
			if (!string.IsNullOrEmpty(role))
			{
				string roleRoom = $"Role_{role}";
				await Groups.AddToGroupAsync(Context.ConnectionId, roleRoom);
			}

			// 3. Gửi một tin nhắn test ngược lại cho đúng cái tab web đó biết là đã join thành công
			await Clients.Caller.SendAsync("ReceiveSystemMessage", $"Kết nối thành công! Đã vào kênh: {userRoom}");
		}

		/// <summary>
		/// Tùy chọn: Hàm này tự chạy khi user tắt tab web hoặc rớt mạng 
		/// </summary>
		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			// Tự động SignalR sẽ rút ConnectionId khỏi các Group, sếp không cần dọn rác thủ công
			// Sếp có thể console.log ra đây để biết user nào vừa offline nếu thích
			await base.OnDisconnectedAsync(exception);
		}
	}
}
