namespace Sparcpoint.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateOnly CurrentFirstDayOfMonth(this DateTime value)
    {
        return new DateOnly(value.Year, value.Month, 1);
    }

    public static int DaysInMonth(this DateTime value)
    {
        return DateTime.DaysInMonth(value.Year, value.Month);
    }

    public static DateOnly CurrentLastDayOfMonth(this DateTime value)
    {
        return new DateOnly(value.Year, value.Month, value.DaysInMonth());
    }
}