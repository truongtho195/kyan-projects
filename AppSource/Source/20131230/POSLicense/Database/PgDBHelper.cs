using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.EntityClient;

namespace POSLicense.Database
{
    class PgDBHelper
    {


        /// <summary>
        /// Dynamic build the connection string for db
        /// i.e. BuildEntityConnString("posadventure", "localhost", "Database.POSDB", "postgres", "postgres")
        /// Store it in ApplicationIsolatedSetting
        /// </summary>
        /// <param name="dbFileName"></param>
        /// <param name="server"></param>
        /// <param name="resourceData"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string BuildConnectionString(string dbFileName, string server, string username, string password)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Port=5432;Encoding=WIN1252;Server=");
            builder.Append(server);
            builder.Append(";Database=");
            builder.Append(dbFileName);
            builder.Append(";UserID=");
            builder.Append(username);
            builder.Append(";Password=");
            builder.Append(password);

            return builder.ToString();
        }

        /// <summary>
        /// Dynamic build the connection string for db
        /// i.e. BuildEntityConnString("posadventure", "localhost", "Database.POSDB", "postgres", "postgres")
        /// Store it in ApplicationIsolatedSetting
        /// </summary>
        /// <param name="dbFileName"></param>
        /// <param name="server"></param>
        /// <param name="resourceData"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static EntityConnectionStringBuilder BuildEntityConnString(string dbFileName, string server, string resourceData, string username, string password)
        {
            string resAll = @"res://*/";
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            entityBuilder.Metadata = string.Format("{0}{1}.csdl|{0}{1}.ssdl|{0}{1}.msl", resAll, resourceData);
            entityBuilder.Provider = "Npgsql";
            entityBuilder.ProviderConnectionString = BuildConnectionString(dbFileName, server, username, password);
            return entityBuilder;
        }

    }
}
