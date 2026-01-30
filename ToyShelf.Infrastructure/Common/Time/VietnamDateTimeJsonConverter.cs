using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ToyShelf.Infrastructure.Common.Time
{
	public class VietnamDateTimeJsonConverter : JsonConverter<DateTime>
	{
		private static readonly TimeZoneInfo VietnamTimeZone =
			TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

		public override DateTime Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options)
		{
			// Client gửi lên → coi là UTC
			return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
		}

		public override void Write(
			Utf8JsonWriter writer,
			DateTime value,
			JsonSerializerOptions options)
		{
			var utc = value.Kind == DateTimeKind.Utc
				? value
				: DateTime.SpecifyKind(value, DateTimeKind.Utc);

			var vietnamTime =
				TimeZoneInfo.ConvertTimeFromUtc(utc, VietnamTimeZone);

			writer.WriteStringValue(
				vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss"));
		}
	}
}
