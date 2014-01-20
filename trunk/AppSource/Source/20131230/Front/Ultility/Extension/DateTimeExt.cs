using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

/// <summary>
/// Process the time zone in the application without the windows.
/// </summary>
static class DateTimeExt
{

    #region Daylight Struct

    public struct Daylight
    {
        public int Year;
        public TimeSpan Delta;
        public DateTime StartDate;
        public DateTime EndDate;
    }

    #endregion

    #region Extension Methods
    static GregorianCalendar _gc = new GregorianCalendar();
    public static int GetWeekOfMonth(this DateTime time)
    {
        DateTime first = new DateTime(time.Year, time.Month, 1);
        return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
    }

    public enum RoundTo
    {
        Second, Minute, Hour, Day
    }

    public static DateTime Round(this DateTime d, RoundTo rt)
    {
        DateTime dtRounded = new DateTime();

        switch (rt)
        {
            case RoundTo.Second:
                dtRounded = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
                if (d.Millisecond >= 500) dtRounded = dtRounded.AddSeconds(1);
                break;
            case RoundTo.Minute:
                dtRounded = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);
                if (d.Second >= 30) dtRounded = dtRounded.AddMinutes(1);
                break;
            case RoundTo.Hour:
                dtRounded = new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0);
                if (d.Minute >= 30) dtRounded = dtRounded.AddHours(1);
                break;
            case RoundTo.Day:
                dtRounded = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
                if (d.Hour >= 12) dtRounded = dtRounded.AddDays(1);
                break;
        }

        return dtRounded;
    }

    public static TimeSpan ToTime(this DateTime date)
    {
        return new TimeSpan(date.Ticks);
    }

    static int GetWeekOfYear(this DateTime time)
    {
        return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
    }

    public static DateTime LastDate(this DateTime dateTime)
    {
        return dateTime.AddMonths(1).AddDays(-1);
    }

    public static DateTime Min(this DateTime dateTime1, DateTime dateTime2)
    {
        if (dateTime1 > dateTime2)
            return dateTime2;
        else
            return dateTime1;
    }

    public static bool Between(this DateTime thisDateTime, DateTime dateTime1, DateTime dateTime2)
    {
        return (thisDateTime >= dateTime1 && thisDateTime <= dateTime2);
    }

    #endregion

    #region Properties

    #region LocalTimeZone
    private static string _localTimeZone = TimeZone.CurrentTimeZone.StandardName;
    public static string LocalTimeZone
    {
        get
        {
            return _localTimeZone;
        }
        set
        {
            if (_localTimeZone != value)
            {
                _localTimeZone = value;
            }
        }
    }

    public static bool IsDaylight
    {
        get;
        set;
    }

    public static void ResetTimezone()
    {
        _localTimeZone = TimeZone.CurrentTimeZone.StandardName;
    }

    /// <summary>
    /// Today Property
    /// </summary>
    public static DateTime Today
    {
        get
        {
            return DateTime.UtcNow.ConvertToLocal(LocalTimeZone).Date;
        }
    }

    /// <summary>
    /// Now Property
    /// </summary>
    public static DateTime Now
    {
        get
        {
            return DateTime.UtcNow.ConvertToLocal(LocalTimeZone);
        }
    }
    #endregion

    #endregion

    #region Private Methods

    /// <summary>
    /// ConvertToUTC
    /// </summary>
    /// <param name="date"></param>
    /// <param name="localTimeZoneKey"></param>
    /// <returns></returns>
    private static DateTime ConvertToUTC(this DateTime date, string localTimeZoneKey)
    {
        // string date = "2009-02-25 16:13:00Z"; // Coordinated Universal Time string from DateTime.Now.ToUniversalTime().ToString("u");
        DateTime localDateTime = date; // Local .NET timeZone. 
        TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneKey);
        DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);

        return utcDateTime;
    }

    /// <summary>
    /// ConvertToUTC Method
    /// </summary>
    /// <param name="date"></param>
    /// <param name="localTimeZoneKey"></param>
    /// <returns></returns>
    private static DateTime ConvertToUTC(this string date, string localTimeZoneKey)
    {
        // string date = "2009-02-25 16:13:00Z"; // Coordinated Universal Time string from DateTime.Now.ToUniversalTime().ToString("u");
        DateTime localDateTime = DateTime.Parse(date); // Local .NET timeZone.
        TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneKey);
        DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);

        return utcDateTime;
    }

    /// <summary>
    /// ConvertToLocal Method
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <param name="localTimeZoneKey"></param>
    /// <returns></returns>
    private static DateTime ConvertToLocal(this DateTime utcDateTime, string localTimeZoneKey)
    {
        if (localTimeZoneKey == null) throw new InvalidTimeZoneException("Invalid Time zone Exception");

        TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneKey);
        DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, localTimeZone);
        if (!IsDaylight)
            return SetDaylight(localDateTime, localTimeZone);

        return localDateTime;
    }

    /// <summary>
    /// SetDayLight Method
    /// </summary>
    /// <param name="currentDate"></param>
    /// <param name="localZone"></param>
    /// <returns></returns>
    private static DateTime SetDaylight(DateTime currentDate, TimeZoneInfo localZone)
    {
        // Get the DaylightTime object for the current year.
        Daylight? daylight = GetDaylightChanges(currentDate.Year, currentDate.Year, localZone);

        if (daylight.HasValue)
        {
            // Display the daylight saving time range for the current year.
            if (currentDate >= daylight.Value.StartDate && currentDate <= daylight.Value.EndDate)
            {
                // daylight.Start, daylight.End, daylight.Delta;
                return currentDate - daylight.Value.Delta;
            }
        }
        return currentDate;
    }

    private static Daylight? GetDaylightChanges(int year, int EndYear, TimeZoneInfo timezoneInfo)
    {
        var Daylight = (from rule in timezoneInfo.GetAdjustmentRules()
                        where rule.DateStart.Year <= year && rule.DateEnd.Year >= year
                        select new
                        {
                            Year = year,
                            StartDate = rule.DaylightTransitionStart,
                            EndDate = rule.DaylightTransitionEnd,
                            Delta = rule.DaylightDelta
                        }).SingleOrDefault();

        if (Daylight == null) return null;

        return new Daylight
            {
                Year = Daylight.Year,
                Delta = Daylight.Delta,
                StartDate = GetDateTime(Daylight.Year, Daylight.StartDate),
                EndDate = GetDateTime(Daylight.Year, Daylight.EndDate)
            };
    }

    private static DateTime GetDateTime(int Year, TimeZoneInfo.TransitionTime transactionTime)
    {
        //Create a datetime to begin with 1st of the transition month
        DateTime dt = new DateTime(Year, transactionTime.Month,
                 1, transactionTime.TimeOfDay.Hour,
                 transactionTime.TimeOfDay.Minute, transactionTime.TimeOfDay.Second);

        //If the dayofweek of 1st is same as the transition day then exit
        //otherwise 
        if (dt.DayOfWeek != transactionTime.DayOfWeek)
        {
            //If transition dayofweek is greater than 1st dayofweek then we need to move further
            //Eg : Transition dayofweek is tuesday and 1st day of week is monday then we need to move 1 day ahead to point to 
            //the transition day
            if (dt.DayOfWeek < transactionTime.DayOfWeek)
            {
                dt.AddDays(transactionTime.DayOfWeek - dt.DayOfWeek);
            }
            else
            {
                //else its not in the 1st week so we move 7 days ahead and move back again
                dt = dt.AddDays(7 - (dt.DayOfWeek - transactionTime.DayOfWeek));
            }
        }

        //Since we are already pointing to the first week of the transition date
        //Add remaining no of weeks to the datetime
        return dt.AddDays((transactionTime.Week - 1) * 7);
    }

    #endregion

    /// <summary>
    /// <para>Truncates a DateTime to a specified resolution.</para>
    /// <para>A convenient source for resolution is TimeSpan.TicksPerXXXX constants.</para>
    /// </summary>
    /// <param name="date">The DateTime object to truncate</param>
    /// <param name="resolution">e.g. to round to nearest second, TimeSpan.TicksPerSecond</param>
    /// <returns>Truncated DateTime</returns>
    public static DateTime Truncate(this DateTime date, long resolution)
    {
        return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
    }

    /// <summary>
    /// <para>Truncates a DateTime to a specified resolution.</para>
    /// <para>A convenient source for resolution is TimeSpan.TicksPerXXXX constants.</para>
    /// </summary>
    /// <param name="date">The DateTime object to truncate</param>
    /// <param name="resolution">e.g. to round to nearest second, TimeSpan.TicksPerSecond</param>
    /// <returns>Truncated DateTime</returns>
    public static TimeSpan Truncate(this TimeSpan time, long resolution)
    {
        return new TimeSpan(time.Ticks - (time.Ticks % resolution));
    }

    ///<summary>Gets the first week day following a date.</summary>
    ///<param name="date">The date.</param>
    ///<param name="dayOfWeek">The day of week to return.</param>
    ///<returns>The first dayOfWeek day following date, or date if it is on dayOfWeek.</returns>
    public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
    {
        return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
    }

    public static DateTime GetNthWeekofMonth(this DateTime date, int nthWeek, DayOfWeek dayOfWeek)
    {
        return date.Next(dayOfWeek).AddDays((nthWeek - 1) * 7);
    }

}
