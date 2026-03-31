namespace PortfolioSite.ViewModels;

public static class AdminDateTimeDisplay
{
    private static readonly TimeZoneInfo JapanTimeZone = ResolveJapanTimeZone();

    public static DateTime ToJst(DateTime value)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utcValue, JapanTimeZone);
    }

    private static TimeZoneInfo ResolveJapanTimeZone()
    {
        foreach (var timeZoneId in new[] { "Asia/Tokyo", "Tokyo Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
