namespace hack;

public class DateHelper
{
    public static DateTime FormatDecades(DateTime dt)
    {
        switch (dt.Day)
        {
            case >= 30: return FormatDecades(dt.AddDays(5));
            case >= 20: return new DateTime(dt.Year, dt.Month, 20);
            case >= 10: return new DateTime(dt.Year, dt.Month, 10);
            default: return new DateTime(dt.Year, dt.Month, 1);
        }
    }
}