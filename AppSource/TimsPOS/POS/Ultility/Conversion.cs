using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Reflection;

namespace CPC.Utility
{
    public class Conversion
    {
        /// <summary>
        /// Convert IEnumerable to DataTable 
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static DataTable EnumerableToTable<T>(IEnumerable<T> collection) where T : class
        {
            DataTable table = new DataTable();
            foreach (T obj in collection)
            {
                PropertyInfo[] propertyInfos = typeof(T).GetProperties();
                if (table.Columns.Count == 0)
                {
                    // This is the first row. create the columns for table (if these are unavailable).
                    foreach (PropertyInfo pi in propertyInfos)
                    {
                        Type pt = pi.PropertyType;

                        // Skip the nullable type
                        if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                            pt = Nullable.GetUnderlyingType(pt);
                        table.Columns.Add(pi.Name, pt);
                    }
                }

                // create data row
                DataRow row = table.NewRow();
                foreach (PropertyInfo pi in propertyInfos)
                {
                    object value = pi.GetValue(obj, null);
                    row[pi.Name] = value ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }
            return table;
        }
    }
}