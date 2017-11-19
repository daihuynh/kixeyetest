using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeUtility
{
    public static double ConvertToUnixTimestamp(DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
        return Math.Floor(diff.TotalSeconds);
    }

    public static double GetCurrentUnixTimestamp() {
        return ConvertToUnixTimestamp(DateTime.UtcNow);
    }
}
