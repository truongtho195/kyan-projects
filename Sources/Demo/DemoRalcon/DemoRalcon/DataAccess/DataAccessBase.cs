using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace DemoFalcon.DataAccess
{
    public class DataAccessBase
    {
        public static string ConnectionString
        {
            get
            {
                string connectionString = "Data Source=" + System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\FalconHRDB.s3db";//
                if (string.IsNullOrEmpty(connectionString) || connectionString.Trim().Length == 0)
                {
                    return string.Empty;
                }
                return connectionString;
            }
        }

        protected void CatchException(Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("\n|| === Exception === ");
            builder.Append("\n|| Source :");
            builder.Append(ex.Source.Replace("\n", "\n||"));
            builder.Append("\n|| Message :");
            builder.Append(ex.Message);
            builder.Append("\n|| StackTrace :");
            builder.Append(ex.StackTrace);
            builder.Append("\n|| TargetSite :");
            builder.Append(ex.TargetSite);
            builder.Append("\n|| Data :");
            foreach (DictionaryEntry item in ex.Data)
            {
                builder.AppendFormat("\n||     {0} : {1}", item.Key, item.Value);
            }
            builder.Append("\n|| All :");
            builder.Append(ex.ToString().Replace("\n", "\n||"));
            builder.AppendFormat("\n|| Data {0}", ex.Data);
            Debug.WriteLine(builder.ToString());
        }
    }
}
