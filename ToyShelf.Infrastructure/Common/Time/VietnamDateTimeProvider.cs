using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Common.Time;

namespace ToyShelf.Infrastructure.Common.Time
{
	public class VietnamDateTimeProvider : IDateTimeProvider
	{
		private static readonly string[] VietnamTimeZoneIds =
		{
			"SE Asia Standard Time",
			"Asia/Ho_Chi_Minh"
		};

		private static readonly TimeZoneInfo VietnamTimeZone =
			ResolveVietnamTimeZone();

		private static TimeZoneInfo ResolveVietnamTimeZone()
		{
			foreach (var id in VietnamTimeZoneIds)
			{
				try
				{
					return TimeZoneInfo.FindSystemTimeZoneById(id);
				}
				catch (TimeZoneNotFoundException)
				{
					// Try next ID.
				}
				catch (InvalidTimeZoneException)
				{
					// Try next ID.
				}
			}

			Console.Error.WriteLine(
				"[VietnamDateTimeProvider] Vietnam time zone not found. Falling back to UTC.");
			return TimeZoneInfo.Utc;
		}

		// Dùng để lưu DB
		public DateTime UtcNow => DateTime.UtcNow;

		// Dùng cho nghiệp vụ / hiển thị
		public DateTime BusinessNow =>
			TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
	}
}
