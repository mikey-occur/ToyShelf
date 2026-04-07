using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ToyShelf.Application.Notifications;

namespace ToyShelf.Infrastructure.Email
{
	public class RedisCacheService : IRedisCacheService
	{
		private readonly IDatabase _db;

		public RedisCacheService(IConnectionMultiplexer redis)
		{
			_db = redis.GetDatabase();
		}

		public async Task<T?> GetAsync<T>(string key)
		{
			var data = await _db.StringGetAsync(key);

			if (data.IsNullOrEmpty)
				return default;

			return JsonSerializer.Deserialize<T>(data!);
		}

		public async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
		{
			var jsonData = JsonSerializer.Serialize(data);

			if (expiry.HasValue)
			{
				// Trích xuất .Value ra (hết null), Redis sẽ tự động hiểu và convert sang Expiration
				await _db.StringSetAsync(key, jsonData, expiry.Value);
			}
			else
			{
				// Nếu không truyền expiry thì không gán thời gian sống
				await _db.StringSetAsync(key, jsonData);
			}
		}

		public async Task RemoveAsync(string key)
		{
			await _db.KeyDeleteAsync(key);
		}
	}
}
