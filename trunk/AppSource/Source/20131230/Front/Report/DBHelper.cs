using System;
using System.Configuration;
using System.Data;
using Npgsql;
using System.Data.EntityClient;

namespace CPC.POS.Report
{
    /// <summary>
    /// Suport get data from data base
    /// </summary>
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
        /// Get data from data base to data table using select query
        /// </summary>
        /// <returns></returns>
        public DataTable ExecuteSelectQuery(string query)
        {
            DataTable dt = new DataTable();
            try
            {
                objConn = new NpgsqlConnection(connString);
                objConn.Open();
                NpgsqlCommand selectCommand = new NpgsqlCommand(query, objConn);
                da = new NpgsqlDataAdapter(selectCommand);
                da.Fill(dt);
            }
            catch (NpgsqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                }
            }
            return dt;
        }

        /// <summary>
        /// Get data from data base to data table using select query
        /// </summary>
        /// <returns></returns>
        public DataTable ExecuteQuery(string table)
        {
            DataTable dt = new DataTable();
            try
            {
                objConn = new NpgsqlConnection(connString);
                objConn.Open();
                string sql = string.Format("SELECT * FROM {0}", table);
                da = new NpgsqlDataAdapter(sql, objConn);
                da.Fill(dt);
            }
            catch (NpgsqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                }

            }
            return dt;
        }
        /// <summary>
        /// Get data from data base to data table using select from view or execute from function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public DataTable ExecuteQuery(string function, string param)
        {
            DataTable dt = new DataTable();
            try
            {
                objConn = new NpgsqlConnection(connString);
                objConn.Open();
                string sql = string.Format("SELECT * FROM {0}({1});", function, param);
                da = new NpgsqlDataAdapter(sql, objConn);
                da.Fill(dt);
            }
            catch (NpgsqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                }

            }
            return dt;
        }

        public int ExecuteNonQuery(string query)
        {
            int i = 0;

            try
            {
                objConn = new NpgsqlConnection(connString);
                objConn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, objConn);
                i = command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                }
            }

            return i;
        }

        public int ExecuteNonQuery(NpgsqlCommand command)
        {
            int i = 0;

            try
            {
                objConn = new NpgsqlConnection(connString);
                objConn.Open();
                command.Connection = objConn;
                i = command.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                }
            }

            return i;
        }

        public DataTable ExecuteQuery(NpgsqlCommand command)
        {
            DataTable dt = new DataTable();

            try
            {
                objConn = new NpgsqlConnection(connString);
                objConn.Open();
                command.Connection = objConn;
                da = new NpgsqlDataAdapter(command);
                da.Fill(dt);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (objConn != null)
                {
                    objConn.Close();
                }
            }

            return dt;
        }

        #endregion
    }
}