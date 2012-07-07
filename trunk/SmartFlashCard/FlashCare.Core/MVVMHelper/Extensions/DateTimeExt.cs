using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

public static class DateTimeExt
{

    static GregorianCalendar _gc = new GregorianCalendar();
    public static int GetWeekOfMonth(this DateTime time)
    {
        DateTime first = new DateTime(time.Year, time.Month, 1);
        return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
    }

    static int GetWeekOfYear(this DateTime time)
    {
        return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
    }

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
    #endregion

    #region IsDayLight
    public static bool IsDaylight
    {
        get;
        set;
    }
    #endregion

    #region Today
    /// <summary>
    /// Today Property
    /// </summary>
    public static DateTime Today
    {
        get
        {
            return ConvertToLocal(DateTime.UtcNow, LocalTimeZone).Date;
        }
    }
    #endregion

    #region Now
    /// <summary>
    /// Now Property
    /// </summary>
    public static DateTime Now
    {
        get
        {
            return ConvertToLocal(DateTime.UtcNow, LocalTimeZone);
        }
    }
    #endregion


    #endregion

    #region Private Convert Methods

    /// <summary>
    /// ConvertToUTC
    /// </summary>
    /// <param name="date"></param>
    /// <param name="localTimeZoneKey"></param>
    /// <returns></returns>
    private static DateTime ConvertToUTC(DateTime date, string localTimeZoneKey)
    {
        // string date = "2009-02-25 16:13:00Z"; // Coordinated Universal Time string from DateTime.Now.ToUniversalTime().ToString("u");
        DateTime localDateTime = date; // Local .NET timeZone. 
        TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneKey);
        DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);

        //string nzTimeZoneKey = "New Zealand Standard Time"; 
        //TimeZoneInfo nzTimeZone = TimeZoneInfo.FindSystemTimeZoneById(nzTimeZoneKey); 
        //DateTime nzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, nzTimeZone);
        return utcDateTime;
    }

    /// <summary>
    /// ConvertToUTC Method
    /// </summary>
    /// <param name="date"></param>
    /// <param name="localTimeZoneKey"></param>
    /// <returns></returns>
    private static DateTime ConvertToUTC(string date, string localTimeZoneKey)
    {
        // string date = "2009-02-25 16:13:00Z"; // Coordinated Universal Time string from DateTime.Now.ToUniversalTime().ToString("u");
        DateTime localDateTime = DateTime.Parse(date); // Local .NET timeZone.
        TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneKey);
        DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);

        //string nzTimeZoneKey = "New Zealand Standard Time"; 
        //TimeZoneInfo nzTimeZone = TimeZoneInfo.FindSystemTimeZoneById(nzTimeZoneKey); 
        //DateTime nzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, nzTimeZone);
        return utcDateTime;
    }

    /// <summary>
    /// ConvertToLocal Method
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <param name="localTimeZoneKey"></param>
    /// <returns></returns>
    private static DateTime ConvertToLocal(DateTime utcDateTime, string localTimeZoneKey)
    {
        // string nzTimeZoneKey = "New Zealand Standard Time";
        if (localTimeZoneKey == null) throw new Exception("Invalid TimeZone");

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
        CustomTimeZone.CustomDaylight? daylight =
            new CustomTimeZone().GetDaylightChanges(currentDate.Year, currentDate.Year, localZone);

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

    #endregion

}
