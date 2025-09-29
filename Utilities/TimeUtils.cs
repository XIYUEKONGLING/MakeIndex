namespace MakeIndex.Utilities;

public static class TimeUtils
{
    public static string GetUtcIsoString()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
    }
    
    public static string ToUtcIsoString(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
    }
    
    public static long GetUnixTimestampMilliseconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    public static long ToUnixTimestampMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }
    
    public static double GetUnixTimestampDouble()
    {
        var now = DateTime.UtcNow;
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var timeSpan = now - unixEpoch;
    
        return timeSpan.TotalSeconds + (now.Millisecond * 1000 + now.Microsecond) / 1000000.0;
    }
    
    public static float GetUnixTimestampFloat()
    {
        return (float)GetUnixTimestampDouble();
    }
    
    public static double ToUnixTimestampDouble(this DateTime dateTime)
    {
        var utcTime = dateTime.ToUniversalTime();
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var timeSpan = utcTime - unixEpoch;
    
        return timeSpan.TotalSeconds + (utcTime.Millisecond * 1000 + utcTime.Microsecond) / 1000000.0;
    }
    
    public static DateTime FromUnixTimestampMilliseconds(long milliseconds)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
    }
    
    public static DateTime FromUnixTimestampDouble(double timestamp)
    {
        var seconds = (long)timestamp;
        var microseconds = (long)((timestamp - seconds) * 1000000);
        
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddSeconds(seconds)
            .AddMicroseconds(microseconds);
    }
    
    public static bool TryParseUtcIsoString(string isoString, out DateTime result)
    {
        return DateTime.TryParseExact(isoString, "yyyy-MM-ddTHH:mm:ss.ffffff", 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.AssumeUniversal, 
            out result);
    }
}