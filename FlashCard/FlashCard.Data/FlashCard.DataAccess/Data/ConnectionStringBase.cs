using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FlashCard.DataAccess
{
    public class ConnectionStringBase
    {
        public static string ConnectionString
        {
            get
            {
                string connectionString = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)+@"\SmartFlashCardDB.s3db";//
                if (string.IsNullOrEmpty(connectionString) || connectionString.Trim().Length == 0)
                {
                    return string.Empty;
                }
                return connectionString;
            }
        }
    }
}
