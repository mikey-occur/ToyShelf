using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
	public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
	{
		public NotificationRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task DeleteOldNotificationsAsync(DateTime thresholdDate)
		{
			await _context.Set<Notification>()
				.Where(n => n.CreatedAt < thresholdDate)
				.ExecuteDeleteAsync();
		}

		public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int take = 20)
		{
			return await _context.Set<Notification>()
				.Where(n => n.UserId == userId)
				.OrderByDescending(n => n.CreatedAt) 
				.Take(take)
				.ToListAsync();
		}
	}
}
