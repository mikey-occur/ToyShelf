using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Notification.Request;
using ToyShelf.Application.Models.Notification.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationController : ControllerBase
	{
		private readonly INotificationService _notificationService;

		public NotificationController(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		/// <summary>
		/// Lấy danh sách 20 thông báo mới nhất của User (Có Cache)
		/// </summary>
		[HttpGet("user/{userId:guid}")]
		public async Task<BaseResponse<List<NotificationResponse>>> GetUserNotifications([FromRoute] Guid userId)
		{
			var result = await _notificationService.GetUserNotificationsAsync(userId);

			return BaseResponse<List<NotificationResponse>>.Ok(result, "Lấy danh sách thông báo thành công");
		}

		/// <summary>
		/// Tạo thông báo mới (Test Push Notification)
		/// </summary>
		[HttpPost]
		public async Task<BaseResponse<string>> CreateNotification([FromBody] CreateNotificationRequest request)
		{
			await _notificationService.CreateNotificationAsync(request);

			// Trả về true hoặc câu thông báo thành công
			return BaseResponse<string>.Ok("Success", "Tạo thông báo mới thành công");
		}

		/// <summary>
		/// Đánh dấu thông báo đã đọc
		/// </summary>
		[HttpPut("{notificationId:guid}/read/user/{userId:guid}")]
		public async Task<BaseResponse<string>> MarkAsRead(
			[FromRoute] Guid notificationId,
			[FromRoute] Guid userId)
		{
			await _notificationService.MarkAsReadAsync(notificationId, userId);

			return BaseResponse<string>.Ok("Success", "Đã đánh dấu đọc thông báo");
		}
	}
}
