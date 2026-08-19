using System.Globalization;

namespace Application.Common;

public static class PersianDateTime
{
    private static readonly PersianCalendar Calendar = new();

    private static readonly Lazy<TimeZoneInfo> IranTimeZone = new(() =>
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
        }
    });

    public static string ToJalaliString(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, IranTimeZone.Value);
        return $"{Calendar.GetYear(local):D4}/{Calendar.GetMonth(local):D2}/{Calendar.GetDayOfMonth(local):D2}";
    }

    public static (DateTime FromUtc, DateTime ToUtc) JalaliDayToUtcRange(string jalaliDate)
    {
        var parts = jalaliDate.Split('/', '-');

        if (parts.Length != 3
            || !int.TryParse(parts[0], out var year)
            || !int.TryParse(parts[1], out var month)
            || !int.TryParse(parts[2], out var day))
            throw new FormatException("Jalali date must be in '1405-05-27' format.");

        DateTime localStart;
        try
        {
            localStart = new DateTime(year, month, day, 0, 0, 0, Calendar);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new FormatException("Invalid Jalali date.");
        }

        if (Calendar.GetYear(localStart) != year || Calendar.GetMonth(localStart) != month || Calendar.GetDayOfMonth(localStart) != day)
            throw new FormatException("Invalid Jalali date.");

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, IranTimeZone.Value);
        return (fromUtc, fromUtc.AddDays(1));
    }
}
