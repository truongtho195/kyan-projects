using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMHelper.Common
{
    public class DateCompare
    {

        #region IsReasonable Method
        //method to check fromDate, toDate of 2DateTime 
        public static bool IsReasonable(DateTime from1, DateTime to1, DateTime from2, DateTime to2)
        {
            if (from1 == from2 || to1 == to2) return false;
            else if (from1 < from2)
            {
                if (to1 > from2 || to1 > to2) return false;
                else return true;
            }
            else
            {
                if (to2 > from1 || to2 > to1) return false;
                else return true;
            }
        } 
        #endregion

        #region GetDayNumber - method to get difference from two date
        public static int GetDayNumber(DateTime? fromDate, DateTime? toDate)
        {
            int dayNum = 0;
            if (fromDate != null && toDate != null)
            {
                DateTime from = (DateTime)fromDate;
                DateTime to = (DateTime)toDate;
                dayNum = ((TimeSpan)(to - from)).Days + 1;
            }
            return dayNum;
        }
        #endregion

    }
}
