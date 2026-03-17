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
				"[VietnamDateTimeJsonConverter] Vietnam time zone not found. Falling back to UTC.");
			return TimeZoneInfo.Utc;
		}

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
