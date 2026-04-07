using Microsoft.AspNetCore.SignalR;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Notification.Request;
using ToyShelf.Application.Models.Notification.Response;
using ToyShelf.Application.Notifications;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class NotificationService : INotificationService
	{
		private readonly INotificationRepository _notificationRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IRedisCacheService _redisCacheService;
		private readonly INotificationBroadcaster _notificationBroadcaster;
		public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork, IRedisCacheService redisCacheService, INotificationBroadcaster notificationBroadcaster)
		{
			_notificationRepository = notificationRepository;
			_unitOfWork = unitOfWork;
			_redisCacheService = redisCacheService;
			_notificationBroadcaster = notificationBroadcaster;
		}

		public async Task CleanupOldNotificationsAsync(int daysToKeep = 30)
		{
			var thresholdDate = DateTime.UtcNow.AddDays(-daysToKeep);

			await _notificationRepository.DeleteOldNotificationsAsync(thresholdDate);
		}

		public async Task CreateNotificationAsync(CreateNotificationRequest request)
		{
			var notification = new Notification
			{
				UserId = request.UserId,
				Title = request.Title,
				Content = request.Content,
				IsRead = false,
				CreatedAt = DateTime.UtcNow
			};

			// Lưu vào Database
			await _notificationRepository.AddAsync(notification);
			await _unitOfWork.SaveChangesAsync();

			// Gọi Service để xóa cache
			string redisKey = $"noti:user:{request.UserId}";
			await _redisCacheService.RemoveAsync(redisKey);
			await _notificationBroadcaster.SendNotificationToUserAsync(request.UserId, notification.Title, notification.Content);
		}

		public async Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId)
		{
			string redisKey = $"noti:user:{userId}";

			// BƯỚC 1: Hỏi Redis qua Service (Code cực ngắn gọn)
			var cachedData = await _redisCacheService.GetAsync<List<NotificationResponse>>(redisKey);
			if (cachedData != null)
			{
				// CACHE HIT: Đã có Data, trả về luôn
				return cachedData;
			}

			// BƯỚC 2: CACHE MISS: Chọc xuống DB lấy (Lấy 20 cái mới nhất)
			var notificationsFromDb = await _notificationRepository.GetByUserIdAsync(userId, 20);

			// Map thủ công từ Entity sang DTO
			var response = notificationsFromDb.Select(n => new NotificationResponse
			{
				Id = n.Id,
				Title = n.Title,
				Content = n.Content,
				IsRead = n.IsRead,
				CreatedAt = n.CreatedAt
			}).ToList();

			if (response.Any())
			{
				await _redisCacheService.SetAsync(redisKey, response, TimeSpan.FromMinutes(15));
			}

			return response;
		}

		public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
		{
			var notification = await _notificationRepository.GetByIdAsync(notificationId);

			
			if (notification != null && notification.UserId == userId && !notification.IsRead)
			{
				notification.IsRead = true;
				_notificationRepository.Update(notification);
				await _unitOfWork.SaveChangesAsync();

				string redisKey = $"noti:user:{userId}";
				await _redisCacheService.RemoveAsync(redisKey);
			}
		}
	}
}
