namespace ToyShelf.Infrastructure.Common.Time
{
    public static class VietnamTimeZoneResolver
    {
        private const string TimeZoneIdEnv = "TOYSHELF_TIMEZONE_ID";
        private const string TimeZoneIdsEnv = "TOYSHELF_TIMEZONE_IDS";

        private static readonly string[] DefaultVietnamTimeZoneIds =
        {
            "Asia/Ho_Chi_Minh",
            "Asia/Bangkok",
            "SE Asia Standard Time"
        };

        private static readonly Lazy<TimeZoneInfo> CachedVietnamTimeZone =
            new(ResolveVietnamTimeZone);

        public static TimeZoneInfo VietnamTimeZone => CachedVietnamTimeZone.Value;

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            foreach (var id in GetCandidateTimeZoneIds())
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
                "[VietnamTimeZoneResolver] Time zone not found. Falling back to UTC.");
            return TimeZoneInfo.Utc;
        }

        private static IEnumerable<string> GetCandidateTimeZoneIds()
        {
            var customTimeZoneIds = Environment.GetEnvironmentVariable(TimeZoneIdsEnv);
            if (!string.IsNullOrWhiteSpace(customTimeZoneIds))
            {
                foreach (var id in customTimeZoneIds.Split(
                    new[] { ',', ';', '|' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    yield return id;
                }
            }

            var customTimeZoneId = Environment.GetEnvironmentVariable(TimeZoneIdEnv);
            if (!string.IsNullOrWhiteSpace(customTimeZoneId))
            {
                yield return customTimeZoneId.Trim();
            }

            foreach (var id in DefaultVietnamTimeZoneIds)
            {
                yield return id;
            }
        }
    }
}