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
            Console.WriteLine("{0} - {1}", DateTime.Now.DayOfWeek, (int)DateTime.Now.DayOfWeek);
            Console.WriteLine("{0} - {1}", DateTime.Now.AddDays(1).DayOfWeek, (int)DateTime.Now.AddDays(1).DayOfWeek);
            Console.WriteLine("{0} - {1}", DateTime.Now.AddDays(2).DayOfWeek, (int)DateTime.Now.AddDays(2).DayOfWeek);
            Console.WriteLine("{0} - {1}", DateTime.Now.AddDays(3).DayOfWeek, (int)DateTime.Now.AddDays(3).DayOfWeek);
            Console.WriteLine("{0} - {1}", DateTime.Now.AddDays(4).DayOfWeek, (int)DateTime.Now.AddDays(4).DayOfWeek);
            Console.WriteLine("{0} - {1}", DateTime.Now.AddDays(5).DayOfWeek, (int)DateTime.Now.AddDays(5).DayOfWeek);
            Console.WriteLine("{0} - {1}", DateTime.Now.AddDays(6).DayOfWeek, (int)DateTime.Now.AddDays(6).DayOfWeek);
            

            //foreach (var item in TimeLogList())
            //{

            //    var result = TotalFullWorkTime(item.LockIn, item.LockOut, true);
            //    Console.WriteLine("ID : {0}", item.ID);
            //    Console.WriteLine("UserName : {0}", item.UserName);
            //    Console.WriteLine("Log  : {0} - {1}", item.LockIn, item.LockOut);
            //    Console.WriteLine("Total FullWorkTime :{0}", result);
            //    var dayOfweek = item.LockIn.Day;
                
                
            //    Console.WriteLine("Total unScheduled :{0}", unScheduled(item.LockIn, item.LockOut,false));

            //    Console.WriteLine("--------------------------------------------------");
            //}


            
            Console.ReadLine();
        }

        private static TimeSpan unScheduled(DateTime clockIn, DateTime clockOut,bool isDeduct)
        {
            DateRange clockRange = new DateRange(clockIn, clockOut);
            DateRange workRange = new DateRange(workIn, workOut);
            DateRange lunchRange = new DateRange(lunchOut, lunchIn);
            DateRange WorkInLunchOut = new DateRange(workIn, lunchOut);
            DateRange lunchInWorkOut = new DateRange(lunchIn, workOut);

            TimeSpan result = new TimeSpan();
            if (clockIn < workIn)
            {
                TimeSpan time = workIn.Subtract(clockIn);
                result = result.Add(time);
            }

            if (clockRange.Intersects(lunchRange) && !isDeduct)
            {
                var time = clockRange.GetIntersection(lunchRange);
                result = result.Add(time.TimeSpan);
            }
           
            if (clockOut > workOut)
            {
                TimeSpan time = clockOut.Subtract(workOut);
                result = result.Add(time);
            }
            return result;
        }

        private static TimeSpan TotalFullWorkTime(DateTime clockIn, DateTime clockOut, bool isdeduct)
        {
            TimeSpan result = new TimeSpan();
            DateRange clockRange = new DateRange(clockIn, clockOut);
            DateRange workRange = new DateRange(workIn, workOut);
            DateRange lunchRange = new DateRange(lunchOut, lunchIn);
            DateRange WorkInLunchOut = new DateRange(workIn, lunchOut);
            DateRange lunchInWorkOut = new DateRange(lunchIn, workOut);
            if (isdeduct)
            {
                if (clockIn < lunchOut & clockOut > lunchIn)
                {
                    var before = clockRange.GetIntersection(WorkInLunchOut);
                    var after = clockRange.GetIntersection(lunchInWorkOut);
                    result = before.TimeSpan.Add(after.TimeSpan);
                }
                else if (clockIn < lunchOut && workOut < lunchIn) //shift 1
                {
                    result = clockRange.GetIntersection(WorkInLunchOut).TimeSpan;
                }
                else if ((clockIn > lunchOut || clockIn < lunchIn) && clockOut > lunchIn)
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


        private static List<TimeLog> TimeLogList()
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
            Console.WriteLine("Total Work Time with Reduce Breake Time :  {0} ", before.TimeSpan.Add(after.TimeSpan));
            //Over time
            DateRange OverTimeRange = new DateRange();
            if (clockRange.Intersects(lunchRange))
                OverTimeRange = clockRange.GetIntersection(lunchRange);


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
