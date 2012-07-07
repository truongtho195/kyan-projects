using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CustomTimeZone
{
    public struct CustomDaylight
    {
        public int Year;
        public TimeSpan Delta;
        public DateTime StartDate;
        public DateTime EndDate;
    }

    public CustomDaylight? GetDaylightChanges(int year, int EndYear, TimeZoneInfo timezoneInfo)
    {
        var DaylightChanges = (from rule in timezoneInfo.GetAdjustmentRules()
                               where rule.DateStart.Year <= year && rule.DateEnd.Year >= year
                               select new
                               {
                                   Year = year,
                                   StartDate = rule.DaylightTransitionStart,
                                   EndDate = rule.DaylightTransitionEnd,
                                   Delta = rule.DaylightDelta
                               }).SingleOrDefault();

        List<CustomDaylight> ListOfDaylight = new List<CustomDaylight>();

        //foreach (var yearChanges in DaylightChanges)
        //{
        //    ListOfDaylight.Add(new CustomDaylight
        //    {
        //        Year = yearChanges.Year,
        //        Delta = yearChanges.Delta,
        //        StartDate = GetDateTime(yearChanges.Year, yearChanges.StartDate),
        //        EndDate = GetDateTime(yearChanges.Year, yearChanges.EndDate)
        //    });
        //}

        //return ListOfDaylight;

        if (DaylightChanges != null)
        {
            return new CustomDaylight
                {
                    Year = DaylightChanges.Year,
                    Delta = DaylightChanges.Delta,
                    StartDate = GetDateTime(DaylightChanges.Year, DaylightChanges.StartDate),
                    EndDate = GetDateTime(DaylightChanges.Year, DaylightChanges.EndDate)
                };
        }

        return null;
    }

    public DateTime GetDateTime(int Year, TimeZoneInfo.TransitionTime transactionTime)
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
}
