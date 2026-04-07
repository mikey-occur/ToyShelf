using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Notifications
{
	public interface IRedisCacheService
	{
		Task<T?> GetAsync<T>(string key);
		Task SetAsync<T>(string key, T data, TimeSpan? expiry = null);
		Task RemoveAsync(string key);
	}
}
