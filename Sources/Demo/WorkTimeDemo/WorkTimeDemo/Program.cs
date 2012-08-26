using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkTimeDemo
{
    class Program
    {

        private static DateTime workIn = new DateTime(2012, 8, 1, 9, 0, 0);
        private static DateTime workOut = new DateTime(2012, 8, 1, 18, 0, 0);
        private static DateTime lunchOut = new DateTime(2012, 8, 1, 12, 0, 0);
        private static DateTime lunchIn = new DateTime(2012, 8, 1, 13, 0, 0);
        static void Main(string[] args)
        {
            DateTime LockIn = new DateTime(2012, 8, 1, 12, 50, 0);
            DateTime LockOut = new DateTime(2012, 8, 1, 16, 30, 0);
            var result = GetFullWorkTime(LockIn, LockOut);
            Console.WriteLine("Full Work Time :  {0}   ||  {1}", result.StartDate, result.EndDate);
            Console.WriteLine("Total FullWorkTime :{0}", result.TimeSpan);
            Console.ReadLine();
        }

        private static TimeSpan TotalFullWorkTime(DateTime clockIn, DateTime clockOut,bool isReduct)
        {
            TimeSpan result = new TimeSpan();
            DateRange clockRange = new DateRange(clockIn, clockOut);
            DateRange workRange = new DateRange(workIn, workOut);
            DateRange lunchRange = new DateRange(lunchOut, lunchIn);
            DateRange WorkInLunchOut = new DateRange(workIn, lunchOut);
            DateRange lunchInWorkOut = new DateRange(lunchIn, workOut);
            if (isReduct)
            {
                if (clockIn < lunchOut & clockOut > lunchIn)
                {
                    var before = clockRange.GetIntersection(WorkInLunchOut);
                    var after = clockRange.GetIntersection(lunchInWorkOut);
                    result = before.TimeSpan.Add(after.TimeSpan);
                }
                else if(clockIn<lunchOut && workOut<lunchIn) //shift 1
                {
                    result = clockRange.GetIntersection(WorkInLunchOut).TimeSpan;
                }
                else if ((clockIn > lunchOut || clockIn < lunchIn) && clockOut >lunchIn)
                {
                    result = clockRange.GetIntersection(lunchInWorkOut).TimeSpan;
                }

            }
            else
            {
                if (clockRange.Intersects(workRange))
                {
                    result = clockRange.GetIntersection(workRange).TimeSpan;
                }
            }
            return result;
        }


        private static DateRange GetFullWorkTime(DateTime clockIn, DateTime clockOut)
        {
            DateRange clockRange = new DateRange(clockIn, clockOut);
            DateRange workRange = new DateRange(workIn, workOut);
            DateRange lunchRange = new DateRange(lunchOut, lunchIn);
            DateRange WorkInLunchOut = new DateRange(workIn, lunchOut);
            DateRange lunchInWorkOut = new DateRange(lunchIn, workOut);
            DateRange FullWorkTime = null;
            if (clockRange.Intersects(WorkInLunchOut))
            {
                FullWorkTime = clockRange.GetIntersection(WorkInLunchOut);
            }
            else if (clockRange.Intersects(lunchInWorkOut))
            {
                FullWorkTime = clockRange.GetIntersection(lunchInWorkOut);
            }
            else if (clockRange.Intersects(workRange))
            {
                FullWorkTime = clockRange.GetIntersection(workRange);
            }
            return FullWorkTime;
        }

        private static TimeSpan TotalWorkTime(DateTime clockIn, DateTime clockOut)
        {
            DateRange clockRange = new DateRange(clockIn, clockOut);
            DateRange workRange = new DateRange(workIn, workOut);
            DateRange lunchRange = new DateRange(lunchOut, lunchIn);
            DateRange WorkInLunchOut = new DateRange(workIn, lunchOut);
            DateRange lunchInWorkOut = new DateRange(lunchIn, workOut);
            TimeSpan result = new TimeSpan();

            return result;
        }


        private List<TimeLog> TimeLogList()
        {
            List<TimeLog> list = new List<TimeLog>();
            list.Add(new TimeLog()
            {
                ID = 1,
                UserName = "Case 1",
                LockIn = new DateTime(2012, 8, 1, 8, 30, 0),
                LockOut = new DateTime(2012, 8, 1, 12, 30, 0)
            });
            list.Add(new TimeLog()
            {
                ID = 1,
                UserName = "Case 1.1",
                LockIn = new DateTime(2012, 8, 1, 12, 50, 0),
                LockOut = new DateTime(2012, 8, 1, 18, 10, 0)
            });
            list.Add(new TimeLog()
            {
                ID = 2,
                UserName = "Case 2",
                LockIn = new DateTime(2012, 8, 1, 8, 50, 0),
                LockOut = new DateTime(2012, 8, 1, 18, 10, 0)
            });

            list.Add(new TimeLog()
            {
                ID = 3,
                UserName = "Case 3",
                LockIn = new DateTime(2012, 8, 1, 9, 50, 0),
                LockOut = new DateTime(2012, 8, 1, 12, 10, 0)
            });
            list.Add(new TimeLog()
            {
                ID = 3,
                UserName = "Case 3.1",
                LockIn = new DateTime(2012, 8, 1, 12, 30, 0),
                LockOut = new DateTime(2012, 8, 1, 18, 10, 0)
            });

            list.Add(new TimeLog()
            {
                ID = 4,
                UserName = "Case 4",
                LockIn = new DateTime(2012, 8, 1, 8, 45, 0),
                LockOut = new DateTime(2012, 8, 1, 10, 10, 0)
            });

            list.Add(new TimeLog()
            {
                ID = 4,
                UserName = "Case 4.1",
                LockIn = new DateTime(2012, 8, 1, 11, 0, 0),
                LockOut = new DateTime(2012, 8, 1, 12, 10, 0)
            });
            list.Add(new TimeLog()
            {
                ID = 4,
                UserName = "Case 4.2",
                LockIn = new DateTime(2012, 8, 1, 12, 50, 0),
                LockOut = new DateTime(2012, 8, 1, 18, 30, 0)
            });

            return list;
        }

        private static void TestOK()
        {
            DateTime clockIn = new DateTime(2012, 8, 1, 10, 30, 0);
            DateTime clockOut = new DateTime(2012, 8, 1, 19, 0, 0);

            DateRange clockRange = new DateRange(clockIn, clockOut);
            DateRange workRange = new DateRange(workIn, workOut);
            DateRange lunchRange = new DateRange(lunchOut, lunchIn);

            //Work Time
            var rangeWorkTime = clockRange.GetIntersection(workRange);
            Console.WriteLine("Work Time :  {0}   ||     {1}", rangeWorkTime.StartDate, rangeWorkTime.EndDate);
            Console.WriteLine("Total Work time :  {0}  ", rangeWorkTime.TimeSpan);

            //Reduct Breake Time
            DateRange WorkInLunchOut = new DateRange(workIn, lunchOut);
            DateRange lunchInWorkOut = new DateRange(lunchIn, workOut);
            var before = clockRange.GetIntersection(WorkInLunchOut);
            var after = clockRange.GetIntersection(lunchInWorkOut);
            Console.WriteLine("Total Work Time with Reduce Breake Time :  {0} ", before.TimeSpan.Value.Add(after.TimeSpan.Value));
            //Over time
            DateRange OverTimeRange = new DateRange();
            if (clockRange.Intersects(lunchRange))
                OverTimeRange = clockRange.GetIntersection(lunchRange);

            if (OverTimeRange.TimeSpan.HasValue)
                Console.WriteLine("OverTime :  {0} ", OverTimeRange.TimeSpan);
        }
    }


    class TimeLog
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public DateTime LockIn { get; set; }
        public DateTime LockOut { get; set; }
    }


}
