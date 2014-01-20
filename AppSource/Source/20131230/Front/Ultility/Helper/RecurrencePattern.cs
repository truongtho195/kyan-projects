using System;
using CPC.POS;

namespace CPC.Helper
{
    public class RecurrencePattern
    {
        /// <summary>
        /// Gets recurrence date base on weekly schedule with format 'Recur every b week(s) on Sunday, Monday, Tuesday...'.
        /// </summary>
        /// <param name="flag">Start date.</param>
        /// <param name="dayOfWeek">WeekDay, Weekend, Monday, Tuesday...</param>
        /// <param name="countWeek">Total week.</param>
        /// <param name="excludeFlag">Exclude or don't exclude start date.</param>
        /// <returns>Recurrence date</returns>
        public static DateTime GetDateBaseOnWeeklySchedule(DateTime flag, WeeklySchedule dayOfWeek, int countWeek, bool excludeFlag = false)
        {
            WeeklySchedule currentDayOfWeek;
            DateTime date = flag;

            if (Enum.TryParse<WeeklySchedule>(date.DayOfWeek.ToString(), out currentDayOfWeek))
            {
                // Neu da qua khoi cai ngay trong tuan chi dinh, thi se tinh
                // cho cac tuan tiep theo. Nguoc lai, tiep tuc tinh tren tuan hien tai.
                if (excludeFlag || currentDayOfWeek > dayOfWeek)
                {
                    // Neu currentDayOfWeek la thu 5, dayOfWeek la thu 4 thi ta khong the cong them 2 tuan
                    // vi nhu vay se vuot mat thu 4 truoc do. Neu countWeek > 1 thi ta chi cong them 1 tuan 
                    // roi tinh tiep, neu countWeek = 1 thi ta bat dau tu hom sau tinh toi.
                    if (currentDayOfWeek > dayOfWeek)
                    {
                        if (countWeek > 1)
                        {
                            date = date.Date.AddDays((countWeek - 1) * 7);
                        }
                        else
                        {
                            date = date.Date.AddDays(GetRemainDayOfWeek(date));
                        }
                    }
                    else
                    {
                        if (countWeek > 1)
                        {
                            date = date.Date.AddDays(countWeek * 7);
                        }
                        else
                        {
                            date = date.Date.AddDays(GetRemainDayOfWeek(date));
                        }
                    }
                }

                // Chon dung ngay trong tuan duoc chi dinh.
                while (date.DayOfWeek.ToString() != dayOfWeek.ToString())
                {
                    date = date.AddDays(1);
                }
            }

            return date.Date;
        }

        /// <summary>
        /// Gets recurrence date base on monthly schedule with format 'Day a of every b month(s)'.
        /// </summary>
        /// <param name="flag">Start date.</param>
        /// <param name="day">Day of month.</param>
        /// <param name="countMonth">Total month.</param>
        /// <param name="excludeFlag">Exclude or don't exclude start date.</param>
        /// <returns>Recurrence date</returns>
        public static DateTime GetDateBaseOnMonthlySchedule(DateTime flag, int day, int countMonth, bool excludeFlag = false)
        {
            DateTime date = flag;
            int daysInMonth = 0;

            // Neu da qua khoi cai ngay duoc chi dinh, thi se tinh cho cac
            // thang tiep theo. Nguoc lai, tiep tuc tinh tren thang hien tai.           
            if (date.Day > day || excludeFlag)
            {
                date = date.AddMonths(countMonth);
            }

            // Tinh so ngay trong thang duoc chon.
            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            // Neu ngay chi dinh hop ly, tuc nho hon so ngay trong thang duoc chon
            // thi lay ngay chi dinh. Nguoc lai thi lay ngay cuoi cung trong thang duoc chon.
            if (day <= daysInMonth)
            {
                return new DateTime(date.Year, date.Month, day);
            }
            return new DateTime(date.Year, date.Month, daysInMonth);
        }

        /// <summary>
        /// Gets recurrence date base on monthly schedule with format 'The Firts Weekend day of every b month(s)'.
        /// </summary>
        /// <param name="flag">Start date.</param>
        /// <param name="order">The order is Firt, second, third...</param>
        /// <param name="dayOfWeek">WeekDay, Weekend, Monday, Tuesday...</param>
        /// <param name="countMonth">Total month.</</param>
        /// <param name="excludeFlag">Exclude or don't exclude start date.</param>
        /// <returns>Recurrence date</returns>
        public static DateTime GetDateBaseOnMonthlySchedule(DateTime flag, WeeksOfMonth order, DaysOfWeek dayOfWeek, int countMonth, bool excludeFlag = false)
        {
            DateTime date = flag;
            int daysInMonth = 0;

            switch (order)
            {
                #region First

                case WeeksOfMonth.First:

                    switch (dayOfWeek)
                    {
                        #region Day

                        // Ngay dau tien cua moi n thang.
                        case DaysOfWeek.Day:

                            // Ngay chi dinh khong phai la ngay dau tien.
                            // Thi tinh cho cac thang tiep theo.
                            if (date.Day > 1 || excludeFlag)
                            {
                                date = date.AddMonths(countMonth);
                                date = new DateTime(date.Year, date.Month, 1);
                            }

                            break;

                        #endregion

                        #region WeekDay

                        // Ngay trong tuan dau tien cua moi n thang.
                        case DaysOfWeek.WeekDay:

                            // Tim ngay trong tuan dau tien cua thang.
                            DateTime? moment = FindWeekDayOfMonth(date.Year, date.Month, 1);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        // Ngay cuoi tuan dau tien cua moi n thang.
                        case DaysOfWeek.WeekendDay:

                            // Tim ngay cuoi tuan dau tien cua thang.
                            moment = FindWeekendOfMonth(date.Year, date.Month, 1);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 1);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        // Ngay chu nhat dau tien cua n thang
                        case DaysOfWeek.Sunday:

                            // Tim ngay chu nhat dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 1, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            // Tim ngay thu 2 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 1, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            // Tim ngay thu 3 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 1, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            // Tim ngay thu 4 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 1, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            // Tim ngay thu 5 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 1, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            // Tim ngay thu 6 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 1, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            // Tim ngay thu 7 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 1, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Second

                case WeeksOfMonth.Second:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            if (date.Day != 2 || excludeFlag)
                            {
                                if (date.Day > 2)
                                {
                                    date = date.AddMonths(countMonth);
                                }
                                date = new DateTime(date.Year, date.Month, 2);
                            }

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            DateTime? moment = FindWeekDayOfMonth(date.Year, date.Month, 2);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            moment = FindWeekendOfMonth(date.Year, date.Month, 2);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 2);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 2, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 2, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 2, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 2, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 2, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 2, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 2, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Third

                case WeeksOfMonth.Third:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            if (date.Day != 3 || excludeFlag)
                            {
                                if (date.Day > 3)
                                {
                                    date = date.AddMonths(countMonth);
                                }
                                date = new DateTime(date.Year, date.Month, 3);
                            }

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            DateTime? moment = FindWeekDayOfMonth(date.Year, date.Month, 3);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            moment = FindWeekendOfMonth(date.Year, date.Month, 3);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 3);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 3, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 3, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 3, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 3, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 3, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 3, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 3, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Fourth

                case WeeksOfMonth.Fourth:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            if (date.Day != 4 || excludeFlag)
                            {
                                if (date.Day > 4)
                                {
                                    date = date.AddMonths(countMonth);
                                }
                                date = new DateTime(date.Year, date.Month, 4);
                            }

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            DateTime? moment = FindWeekDayOfMonth(date.Year, date.Month, 4);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            moment = FindWeekendOfMonth(date.Year, date.Month, 4);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 4);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            // Tim ngay chu nhat dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, date.Month, 4, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 4, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 4, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 4, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 4, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 4, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            moment = FindWeekDayOfMonth(date.Year, date.Month, 4, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddMonths(countMonth);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Last

                case WeeksOfMonth.Last:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            if (excludeFlag)
                            {
                                date = date.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            }
                            date = new DateTime(date.Year, date.Month, daysInMonth);

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            DateTime moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay trong tuan cuoi cung cua thang.
                            while (moment.DayOfWeek == DayOfWeek.Saturday || moment.DayOfWeek == DayOfWeek.Sunday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay trong tuan cuoi cung cua thang.
                                while (moment.DayOfWeek == DayOfWeek.Saturday || moment.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay cuoi tuan cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Saturday && moment.DayOfWeek != DayOfWeek.Sunday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay trong tuan cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Saturday && moment.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay chu nhat cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Sunday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay chu nhat cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay thu 2 cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Monday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay thu 2 cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Monday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay thu 3 cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Tuesday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay thu 3 cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Tuesday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay thu 4 cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Wednesday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay thu 4 cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Wednesday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay thu 5 cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Thursday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay thu 5 cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Thursday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay thu 6 cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Friday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay thu 6 cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Friday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, date.Month, daysInMonth);
                            // Tim ngay thu 7 cuoi cung cua thang.
                            while (moment.DayOfWeek != DayOfWeek.Saturday)
                            {
                                moment = moment.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Date || excludeFlag)
                            {
                                // Tim o nhung thang tiep theo.
                                moment = moment.AddMonths(countMonth);
                                daysInMonth = DateTime.DaysInMonth(moment.Year, moment.Month);
                                moment = new DateTime(moment.Year, moment.Month, daysInMonth);
                                // Tim ngay thu 7 cuoi cung cua thang.
                                while (moment.DayOfWeek != DayOfWeek.Saturday)
                                {
                                    moment = moment.AddDays(-1);
                                }
                            }
                            date = moment.Date;

                            break;

                        #endregion
                    }

                    break;

                #endregion
            }

            return date.Date;
        }

        /// <summary>
        /// Gets recurrence date base on yearly schedule with format 'Every February 28'.
        /// </summary>
        /// <param name="flag">Start date.</param>
        /// <param name="monthOfYear">Month of year.</param>
        /// <param name="day">Day of month.</param>
        /// <param name="excludeFlag">Exclude or don't exclude start date.</param>
        /// <returns>Recurrence date</returns>
        public static DateTime GetDateBaseOnYearlySchedule(DateTime flag, MonthOfYear monthOfYear, int day, bool excludeFlag = false)
        {
            DateTime date = flag;
            DateTime moment = new DateTime(date.Year, (int)monthOfYear, day);
            if (date.Date > moment.Date || excludeFlag)
            {
                moment = moment.AddYears(1);
            }
            date = moment;
            return date.Date;
        }

        /// <summary>
        /// Gets recurrence date base on yearly schedule with format 'The First Weekday of January'.
        /// </summary>
        /// <param name="flag">Start date.</param>
        /// <param name="order">The order is Firt, second, third...</param>
        /// <param name="dayOfWeek">WeekDay, Weekend, Monday, Tuesday...</param>
        /// <param name="monthOfYear">Month of year.</param>
        /// <param name="excludeFlag">Exclude or don't exclude start date.</param>
        /// <returns>Recurrence date</returns>
        public static DateTime GetDateBaseOnYearlySchedule(DateTime flag, WeeksOfMonth order, DaysOfWeek dayOfWeek, MonthOfYear monthOfYear, bool excludeFlag = false)
        {
            DateTime date = flag;
            int daysInMonth = 0;

            switch (order)
            {
                #region First

                case WeeksOfMonth.First:

                    switch (dayOfWeek)
                    {
                        #region Day

                        // Ngay dau tien cua thang.
                        case DaysOfWeek.Day:

                            DateTime? moment = new DateTime(date.Year, (int)monthOfYear, 1);
                            // Neu ngay nay da qua
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                moment = moment.Value.AddYears(1);
                            }
                            date = moment.Value;

                            break;

                        #endregion

                        #region WeekDay

                        // Ngay trong tuan dau tien cua thang.
                        case DaysOfWeek.WeekDay:

                            // Tim ngay trong tuan dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nam sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        // Ngay cuoi tuan dau tien cua moi n thang.
                        case DaysOfWeek.WeekendDay:

                            // Tim ngay cuoi tuan dau tien cua thang.
                            moment = FindWeekendOfMonth(date.Year, (int)monthOfYear, 1);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 1);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        // Ngay chu nhat dau tien cua n thang
                        case DaysOfWeek.Sunday:

                            // Tim ngay chu nhat dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            // Tim ngay thu 2 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            // Tim ngay thu 3 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            // Tim ngay thu 4 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            // Tim ngay thu 5 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            // Tim ngay thu 6 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            // Tim ngay thu 7 dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 1, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 1, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Second

                case WeeksOfMonth.Second:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            DateTime? moment = new DateTime(date.Year, (int)monthOfYear, 2);
                            // Neu ngay nay da qua
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                moment = moment.Value.AddYears(1);
                            }
                            date = moment.Value;

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            moment = FindWeekendOfMonth(date.Year, (int)monthOfYear, 2);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 2);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 2, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 2, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Third

                case WeeksOfMonth.Third:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            DateTime? moment = new DateTime(date.Year, (int)monthOfYear, 3);
                            // Neu ngay nay da qua
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                moment = moment.Value.AddYears(1);
                            }
                            date = moment.Value;

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            moment = FindWeekendOfMonth(date.Year, (int)monthOfYear, 3);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 3);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 3, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 3, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Fourth

                case WeeksOfMonth.Fourth:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            DateTime? moment = new DateTime(date.Year, (int)monthOfYear, 4);
                            // Neu ngay nay da qua
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                moment = moment.Value.AddYears(1);
                            }
                            date = moment.Value;

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            moment = FindWeekendOfMonth(date.Year, (int)monthOfYear, 4);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekendOfMonth(moment.Value.Year, moment.Value.Month, 4);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            // Tim ngay chu nhat dau tien cua thang.
                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4, DayOfWeek.Sunday);
                            if (moment.HasValue)
                            {
                                // Neu ngay nay da qua di.
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    // Tim o nhug thang sau.
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Sunday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4, DayOfWeek.Monday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Monday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4, DayOfWeek.Tuesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Tuesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4, DayOfWeek.Wednesday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Wednesday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4, DayOfWeek.Thursday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Thursday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4, DayOfWeek.Friday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Friday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            moment = FindWeekDayOfMonth(date.Year, (int)monthOfYear, 4, DayOfWeek.Saturday);
                            if (moment.HasValue)
                            {
                                if (date.Date > moment.Value.Date || excludeFlag)
                                {
                                    moment = moment.Value.AddYears(1);
                                    moment = FindWeekDayOfMonth(moment.Value.Year, moment.Value.Month, 4, DayOfWeek.Saturday);
                                    if (moment.HasValue)
                                    {
                                        date = moment.Value.Date;
                                    }
                                    else
                                    {
                                        throw new Exception("Date not found");
                                    }
                                }
                                else
                                {
                                    date = moment.Value.Date;
                                }
                            }
                            else
                            {
                                throw new Exception("Date not found");
                            }

                            break;

                        #endregion
                    }

                    break;

                #endregion

                #region Last

                case WeeksOfMonth.Last:

                    switch (dayOfWeek)
                    {
                        #region Day

                        case DaysOfWeek.Day:

                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            DateTime? moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Neu ngay nay da qua
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                moment = moment.Value.AddYears(1);
                            }
                            date = moment.Value;

                            break;

                        #endregion

                        #region WeekDay

                        case DaysOfWeek.WeekDay:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay trong tuan cuoi cung cua thang.
                            while (moment.Value.DayOfWeek == DayOfWeek.Saturday || moment.Value.DayOfWeek == DayOfWeek.Sunday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung bam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay trong tuan cuoi cung cua thang.
                                while (moment.Value.DayOfWeek == DayOfWeek.Saturday || moment.Value.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region WeekendDay

                        case DaysOfWeek.WeekendDay:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay cuoi tuan cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Saturday && moment.Value.DayOfWeek != DayOfWeek.Sunday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay trong tuan cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Saturday && moment.Value.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region Sunday

                        case DaysOfWeek.Sunday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay chu nhat cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Sunday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay chu nhat cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Sunday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region Monday

                        case DaysOfWeek.Monday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay thu 2 cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Monday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay thu 2 cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Monday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region Tuesday

                        case DaysOfWeek.Tuesday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay thu 2 cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Tuesday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay thu 2 cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Tuesday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region Wednesday

                        case DaysOfWeek.Wednesday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay thu 2 cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Wednesday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay thu 2 cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Wednesday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region Thursday

                        case DaysOfWeek.Thursday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay thu 2 cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Thursday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay thu 2 cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Thursday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region Friday

                        case DaysOfWeek.Friday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay thu 2 cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Friday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay thu 2 cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Friday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion

                        #region Saturday

                        case DaysOfWeek.Saturday:

                            // Tinh so ngay trong thang duoc chon.
                            daysInMonth = DateTime.DaysInMonth(date.Year, (int)monthOfYear);
                            // Ngay cuoi cung trong thang.
                            moment = new DateTime(date.Year, (int)monthOfYear, daysInMonth);
                            // Tim ngay thu 2 cuoi cung cua thang.
                            while (moment.Value.DayOfWeek != DayOfWeek.Saturday)
                            {
                                moment = moment.Value.AddDays(-1);
                            }

                            // Neu ngay nay da qua di.
                            if (date.Date > moment.Value.Date || excludeFlag)
                            {
                                // Tim o nhung nam tiep theo.
                                moment = moment.Value.AddYears(1);
                                daysInMonth = DateTime.DaysInMonth(moment.Value.Year, moment.Value.Month);
                                moment = new DateTime(moment.Value.Year, moment.Value.Month, daysInMonth);
                                // Tim ngay thu 2 cuoi cung cua thang.
                                while (moment.Value.DayOfWeek != DayOfWeek.Saturday)
                                {
                                    moment = moment.Value.AddDays(-1);
                                }
                            }
                            date = moment.Value.Date;

                            break;

                        #endregion
                    }

                    break;

                #endregion
            }

            return date.Date;
        }

        /// <summary>
        /// Finds weekday of month.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Date of weekday.</returns>
        public static DateTime? FindWeekDayOfMonth(int year, int month, int offset)
        {
            if (offset < 1 || offset > DateTime.DaysInMonth(year, month))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            DateTime moment = new DateTime(year, month, 1);
            while (moment.Month == month)
            {
                DayOfWeek dayOfWeek = moment.DayOfWeek;
                if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
                {
                    offset--;
                }
                if (offset == 0)
                {
                    return moment;
                }
                moment = moment.AddDays(1);
            }

            return null;
        }

        /// <summary>
        /// Finds weekday of month.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="dayOfWeek">WeekDay, Weekend, Monday, Tuesday...</param>
        /// <returns>Date of weekday.</returns>
        public static DateTime? FindWeekDayOfMonth(int year, int month, int offset, DayOfWeek dayOfWeek)
        {
            if (offset < 1 || offset > DateTime.DaysInMonth(year, month))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            DateTime moment = new DateTime(year, month, 1);
            while (moment.Month == month)
            {
                DayOfWeek dayOfWeekMoment = moment.DayOfWeek;
                if (dayOfWeekMoment == dayOfWeek)
                {
                    offset--;
                }
                if (offset == 0)
                {
                    return moment;
                }
                moment = moment.AddDays(1);
            }

            return null;
        }

        /// <summary>
        /// Finds weekend of month.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Date of weekend.</returns>
        public static DateTime? FindWeekendOfMonth(int year, int month, int offset)
        {
            if (offset < 1 || offset > DateTime.DaysInMonth(year, month))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            DateTime moment = new DateTime(year, month, 1);
            while (moment.Month == month)
            {
                DayOfWeek dayOfWeek = moment.DayOfWeek;
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    offset--;
                }
                if (offset == 0)
                {
                    return moment;
                }
                moment = moment.AddDays(1);
            }

            return null;
        }

        /// <summary>
        /// Gets remain day of week.
        /// </summary>
        /// <param name="flag">Start date.</param>
        /// <returns>Total remain days.</returns>
        public static short GetRemainDayOfWeek(DateTime flag)
        {
            short count = 1;
            flag = flag.AddDays(1);
            while (flag.DayOfWeek.ToString() != DayOfWeek.Sunday.ToString())
            {
                count++;
                flag = flag.AddDays(1);
            }
            return count;
        }
    }
}
