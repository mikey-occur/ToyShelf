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
                catch (Exception ex) when (IsLookupFailure(ex))
                {
                    Console.Error.WriteLine(
                        $"[VietnamTimeZoneResolver] Failed time zone id '{id}': {ex.GetType().Name}. Trying next.");
                }
            }

            Console.Error.WriteLine(
                "[VietnamTimeZoneResolver] Time zone not found. Falling back to UTC.");
            return TimeZoneInfo.Utc;
        }

        private static IEnumerable<string> GetCandidateTimeZoneIds()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var customTimeZoneIds = Environment.GetEnvironmentVariable(TimeZoneIdsEnv);
            if (!string.IsNullOrWhiteSpace(customTimeZoneIds))
            {
                foreach (var id in customTimeZoneIds.Split(
                    new[] { ',', ';', '|' },
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (seen.Add(id))
                    {
                        yield return id;
                    }
                }
            }

            var customTimeZoneId = Environment.GetEnvironmentVariable(TimeZoneIdEnv);
            if (!string.IsNullOrWhiteSpace(customTimeZoneId))
            {
                var trimmed = customTimeZoneId.Trim();
                if (seen.Add(trimmed))
                {
                    yield return trimmed;
                }
            }

            foreach (var id in DefaultVietnamTimeZoneIds)
            {
                if (seen.Add(id))
                {
                    yield return id;
                }
            }
        }

        private static bool IsLookupFailure(Exception ex)
        {
            return ex is TimeZoneNotFoundException
                || ex is InvalidTimeZoneException
                || ex is IOException
                || ex is UnauthorizedAccessException;
        }
    }
}