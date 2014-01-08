using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Npgsql;
using System.Configuration;
using System.Data.EntityClient;
using Xceed.Wpf.Toolkit;

namespace CPC.POSReport.Function
{
    public class DBHelper
    {
        string connString = string.Empty;
        NpgsqlConnection objConn;
        NpgsqlDataAdapter da;

        #region -Contructor-
        public DBHelper()
        {
            connString = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["POSDBEntities"].ConnectionString).ProviderConnectionString;            
        }
        #endregion

        #region -Public method-
        /// <summary>
        /// Get data from data base to data table
        /// </summary>
        /// <param name="tableName">Tatble Name in database</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteQuery(string tableName)
        {
            DataTable dt = new DataTable();
            try
            {
                OpenConnection();
                string sql = string.Format("SELECT * FROM {0}", tableName);
                da = new NpgsqlDataAdapter(sql, objConn);
                da.Fill(dt);
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                CloseConnection();
            }
            return dt;
        }

        /// <summary>
        /// Execute Function to get data to DataTable
        /// </summary>
        /// <param name="vieworFunctionName">Name of view or Function</param>
        /// <param name="param">Parameter</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteQuery(string vieworFunctionName, string param)
        {
            DataTable dt = new DataTable();
            try
            {
                OpenConnection();
                string sql = string.Format("SELECT * FROM {0}({1});", vieworFunctionName, param);
                da = new NpgsqlDataAdapter(sql, objConn);
                da.Fill(dt);
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                CloseConnection();
            }
            return dt;
        }
      
		// Open connecition
        private void OpenConnection()
        {
            objConn = new NpgsqlConnection(connString);
            if (objConn != null && objConn.State != ConnectionState.Open)
            {
                objConn.Open();
            }            
        }
		// Close Connection
        private void CloseConnection()
        {            
            if (objConn != null && objConn.State == ConnectionState.Open)
            {
                objConn.Close();
            }
        }

        #endregion
    }
}
