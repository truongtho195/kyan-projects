using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Reflection;

public class ConvertData
{
    /// <summary>
    /// Convert IEnumerable to DataTable 
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static DataTable IEnumerableToDataTable(IEnumerable collection)
    {
        DataTable dt = new DataTable();
        foreach (object obj in collection)
        {
            Type type = obj.GetType();
            PropertyInfo[] propertyInfos = type.GetProperties();
            if (dt.Columns.Count == 0)
            {
                foreach (PropertyInfo pi in propertyInfos)
                {
                    if (pi.Name.Contains("Error") || pi.Name == "Item") continue;
                    Type pt = pi.PropertyType;
                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                        pt = Nullable.GetUnderlyingType(pt);
                    dt.Columns.Add(pi.Name, pt);
                }
            }
            DataRow dr = dt.NewRow();
            foreach (PropertyInfo pi in propertyInfos)
            {
                if (pi.Name.Contains("Error") || pi.Name == "Item") continue;
                object value = pi.GetValue(obj, null);
                dr[pi.Name] = value ?? DBNull.Value;
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }

    /// <summary>
    /// Substract two minutes strings 
    /// </summary>
    /// <param name="input1"></param>
    /// <param name="input2"></param>
    /// <returns></returns>
    public static int SubstractToMinutes(string input1, string input2)
    {
        string[] parts1 = input1.Split(':');
        int day1 = int.Parse(parts1[0]); // Weekday
        int hour1 = int.Parse(parts1[1]);
        int minute1 = int.Parse(parts1[2]);
        int t1 = int.Parse(parts1[3]);

        int s1 = parts1.Length == 4 ? 0 : int.Parse(parts1[4]);

        string[] parts2 = input2.Split(':');
        int day2 = int.Parse(parts2[0]); // Weekday
        int hour2 = int.Parse(parts2[1]);
        int minute2 = int.Parse(parts2[2]);
        int t2 = int.Parse(parts2[3]);

        int s2 = parts2.Length == 4 ? 0 : int.Parse(parts2[4]);

        // Processing hours.
        // 12 hours
        if (t1 == 0)
        {
            // AM
            if (hour1 == 12)
                hour1 = 0;
        }
        else
        {
            // PM - Range in 0 - 11
            if (hour1 != 12)
                hour1 += 12;
        }

        if (t2 == 0)
        {
            // AM
            if (hour2 == 12)
                hour2 = 0;
        }
        else
        {
            // PM
            if (hour2 != 12)
                hour2 += 12;
        }

        // Processing days to hours.
        if (day2 != day1)
        {
            if (day2 > day1)
            {
                // Saturday && Sunday
                if (day2 == 6 && day1 == 0)
                    hour1 += 24;
                else
                    hour2 += 24;
            }
            else
            {
                // Sunday && Saturday
                if (day2 == 0 && day1 == 6)
                    hour2 += 24;
                else
                    hour1 += 24;
            }
        }

        // Processing minutes
        if (s2 > s1)
            minute2++;
        else if (s2 < s1)
            minute1++;

        return (minute2 - minute1) + (hour2 - hour1) * 60;
    }

}

