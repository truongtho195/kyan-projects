using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Number conversion
/// </summary>
static class NumberUtils
{
    public static TimeSpan ToTimeSpan(this float minutes)
    {
        return TimeSpan.FromMinutes(minutes);
    }

    public static string ToHours(this float minutes)
    {
        return minutes.ToTimeSpan().ToHours();
    }

    public static string ToHoursNonZero(this float minutes)
    {
        return minutes > 0 ? minutes.ToTimeSpan().ToHours() : string.Empty;
    }

    public static string ToHours(this TimeSpan ts)
    {
        var hour = (long)ts.TotalMinutes / 60;
        var minutes = Math.Abs(ts.TotalMinutes % 60);
        return String.Format("{0}:{1:00}", hour, minutes);
    }

    public static float RoundTime(this float f)
    {
        return (float)Math.Round(f);
    }

    public static string Ordinal(this int number)
    {
        var ones = number % 10;
        var tens = Math.Floor(number / 10f) % 10;
        if (tens == 1)
        {
            return number + "th";
        }

        switch (number % 10)
        {
            case 1: return number + "st";
            case 2: return number + "nd";
            case 3: return number + "rd";
            default: return number + "th";
        }
    }
}