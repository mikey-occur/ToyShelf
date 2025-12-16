using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Common.Time
{
	public interface IDateTimeProvider
	{
		/// <summary>
		/// Thời gian chuẩn để LƯU DB (UTC)
		/// </summary>
		DateTime UtcNow { get; }

		/// <summary>
		/// Thời gian nghiệp vụ (giờ Việt Nam)
		/// ❗ Chỉ dùng cho logic / hiển thị
		/// </summary>
		DateTime BusinessNow { get; }
	}
}
