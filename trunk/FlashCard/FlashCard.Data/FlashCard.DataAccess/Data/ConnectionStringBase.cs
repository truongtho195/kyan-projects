﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlashCard.DataAccess
{
    public class ConnectionStringBase
    {
        public static string ConnectionString
        {
            get
            {
                string connectionString = "string";//
                if (string.IsNullOrEmpty(connectionString) || connectionString.Trim().Length == 0)
                {
                    return string.Empty;
                }
                return connectionString;
            }
        }
    }
}