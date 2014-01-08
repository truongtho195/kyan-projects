using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;

namespace POSLicense.Database
{
    class ADODatabase
    {
        private string _conString = string.Empty;
        private NpgsqlConnection _pgConnection;
        private NpgsqlDataAdapter _pgAdapt;
        public ADODatabase(string cnn)
        {
            _conString = cnn;
        }

        public DataTable ExecuteQuery(string table)
        {
            DataTable dt = new DataTable();
            try
            {
                _pgConnection = new NpgsqlConnection(_conString);
                _pgConnection.Open();
                string sql = string.Format("select * from \"{0}\"", table);
                _pgAdapt = new NpgsqlDataAdapter(sql, _pgConnection);
                _pgAdapt.Fill(dt);
            }
            catch (NpgsqlException ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

            finally
            {
                if (_pgConnection != null)
                {
                    _pgConnection.Close();
                }

            }
            return dt;
        }
    }
}
