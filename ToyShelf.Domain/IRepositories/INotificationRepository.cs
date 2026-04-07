using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface INotificationRepository : IGenericRepository<Notification>
	{
		Task<List<Notification>> GetByUserIdAsync(Guid userId, int take = 20);
		Task DeleteOldNotificationsAsync(DateTime thresholdDate);
	}
}
