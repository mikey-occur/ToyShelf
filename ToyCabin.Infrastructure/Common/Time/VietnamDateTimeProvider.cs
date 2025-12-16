using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Common.Time;

namespace ToyCabin.Infrastructure.Common.Time
{
	public class VietnamDateTimeProvider : IDateTimeProvider
	{
		private static readonly TimeZoneInfo VietnamTimeZone =
			TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

		// Dùng để lưu DB
		public DateTime UtcNow => DateTime.UtcNow;

		// Dùng cho nghiệp vụ / hiển thị
		public DateTime BusinessNow =>
			TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
	}
}
