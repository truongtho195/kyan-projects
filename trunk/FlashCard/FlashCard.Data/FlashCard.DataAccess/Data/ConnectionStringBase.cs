using System.IO;

namespace FlashCard.DataAccess
{
    public class ConnectionStringBase
    {
        public static string ConnectionString
        {
            get
            {
                string connectionString = "Data Source=" + Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\SmartFlashCardDB.s3db";
                if (string.IsNullOrEmpty(connectionString) || connectionString.Trim().Length == 0)
                {
                    return string.Empty;
                }
                return connectionString;
            }
        }
    }
}
