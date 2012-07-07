using System;
using System.Text;
using System.Collections;
using System.IO;

namespace FlashCard.DataAccess
{
    public class DataAccessBase
    {
        public DataAccessBase()
        {
            DebugShowErrorMsg = true;
        }
        public bool DebugShowErrorMsg { get; set; }

        public static string ConnectionString
        {
            get
            {
                //string connectionString = "Data Source=" + Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\SmartFlashCardDB.s3db";//
                string connectionString = "Data Source=" + Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\SmartFlashCardDB.s3db";
                if (string.IsNullOrEmpty(connectionString) || connectionString.Trim().Length == 0)
                {
                    return string.Empty;
                }
                return connectionString;
            }
        }

        protected string CatchException(Exception ex)
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
            return builder.ToString();
        }


    }
}
