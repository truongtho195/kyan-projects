using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DemoFalcon.Model;

namespace DemoFalcon.DataAccess
{
    public class CountryDataAccess : DataAccessBase
    {
        #region Contructors
        public CountryDataAccess()
        {
        }
        #endregion

        #region Methods
        public CountryModel Get(int countryID)
        {
            CountryModel countryModel = new CountryModel();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            string sql = "select * From Countries where CountryID ==@CountryID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@CountryID", countryID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    countryModel = GetCountryModel(reader);
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
                throw;
            }
            finally
            {
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return countryModel;
        }

        public IList<CountryModel> GetAll()
        {
            List<CountryModel> list = new List<CountryModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;

            SQLiteDataReader reader = null;
            string sql = "select * From Countries";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;

                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    CountryModel employeeModel = GetCountryModel(reader);
                    list.Add(employeeModel);
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
                throw;
            }
            finally
            {
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return list;
        }

        //public IList<CountryModel> GetAll(CountryModel backSide)
        //{
        //    List<CountryModel> list = new List<CountryModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    string sql = "select * from Countrys ";

        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        string sqlcondition = string.Empty;
        //        if (backSide.CountryID > -1)
        //        {
        //            if (string.IsNullOrWhiteSpace(sqlcondition))
        //                sqlcondition += "where CountryID==@backSideID";
        //            else
        //                sqlcondition += "&& CountryID==@backSideID";
        //            SQLiteParameter param = new SQLiteParameter("@backSideID", backSide.CountryID);
        //            sqlCommand.Parameters.Add(param);
        //        }
        //        if (backSide.LessonID > -1)
        //        {
        //            if (string.IsNullOrWhiteSpace(sqlcondition))
        //                sqlcondition += "where LessonID==@lessonID";
        //            else
        //                sqlcondition += "&& LessonID==@lessonID";
        //            SQLiteParameter param = new SQLiteParameter("@lessonID", backSide.LessonID);
        //            sqlCommand.Parameters.Add(param);
        //        }
        //        sqlCommand.CommandText = sql + sqlcondition;
        //        reader = sqlCommand.ExecuteReader();

        //        while (reader.Read())
        //        {
        //            CountryModel backSideModel = GetCountryModel(reader);
        //            list.Add(backSideModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CatchException(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        sqlConnect.Dispose();
        //        sqlCommand.Dispose();
        //        reader.Dispose();
        //    }
        //    return list;
        //}




        //public IList<CountryModel> GetAllWithRelation()
        //{
        //    List<CountryModel> list = new List<CountryModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    LessonDataAccess lessonDA = new LessonDataAccess();
        //    CategoryDataAccess categoryDA = new CategoryDataAccess();
        //    TypeDataAccess typeDA = new TypeDataAccess();
        //    string sql = "select * from Countrys ";
        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        sqlCommand.CommandText = sql;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            //CountryModel
        //            CountryModel backSideModel = GetCountryModel(reader);
        //            //LessonModel
        //            backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
        //            //CategoryModel
        //            backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            //TypeModel
        //            backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            list.Add(backSideModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CatchException(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        sqlConnect.Dispose();
        //        sqlCommand.Dispose();
        //        reader.Dispose();
        //    }
        //    return list;
        //}

        //public IList<CountryModel> GetAllWithRelation(int backSideID)
        //{
        //    List<CountryModel> list = new List<CountryModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    SQLiteParameter param = null;
        //    LessonDataAccess lessonDA = new LessonDataAccess();
        //    CategoryDataAccess categoryDA = new CategoryDataAccess();
        //    TypeDataAccess typeDA = new TypeDataAccess();
        //    string sql = "select * from Countrys where CountryID==@backSideID";
        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        sqlCommand.CommandText = sql;
        //        param = new SQLiteParameter("@backSideID", backSideID);
        //        sqlCommand.Parameters.Add(param);
        //        reader = sqlCommand.ExecuteReader();
        //        CountryModel backSideModel;
        //        while (reader.Read())
        //        {
        //            //CountryModel
        //            backSideModel = GetCountryModel(reader);
        //            //LessonModel
        //            backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
        //            //CategoryModel
        //            backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            //TypeModel
        //            backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            list.Add(backSideModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CatchException(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        sqlConnect.Dispose();
        //        sqlCommand.Dispose();
        //        reader.Dispose();
        //    }
        //    return list;
        //}


        public bool Insert(CountryModel countryModel)
        {
            bool result = false;
            string sql = "insert into Countries (CountryName) values (@CountryName)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@CountryID", countryModel.CountryID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@CountryName", countryModel.CountryName));
                sqlCommand.ExecuteNonQuery();
                countryModel.CountryID = (int)sqlConnect.LastInsertRowId;
                countryModel.IsNew = false;
                countryModel.IsDelete = false;
                countryModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                CatchException(ex);
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }


        public bool Update(CountryModel employeeModel)
        {
            bool result = false;
            string sql = "Update Countries setCountryName=@CountryName where CountryID = @CountryID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@CountryID", employeeModel.CountryID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@CountryName", employeeModel.CountryName));

                sqlCommand.ExecuteNonQuery();
                employeeModel.IsNew = false;
                employeeModel.IsDelete = false;
                employeeModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                CatchException(ex);
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }

        public bool Delete(CountryModel employeeModel)
        {
            bool result = false;
            string sql = "Delete Countries where CountryID = @CountryID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@CountryID", employeeModel.CountryID));
                sqlCommand.ExecuteNonQuery();
                employeeModel.IsNew = false;
                employeeModel.IsDelete = false;
                employeeModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                CatchException(ex);
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }


        private CountryModel GetCountryModel(SQLiteDataReader reader)
        {
            CountryModel employeeModel = new CountryModel();
            employeeModel.CountryID = reader["CountryID"] == null ? 0 : int.Parse(reader["CountryID"].ToString());
            employeeModel.CountryName = reader["CountryName"] == null ? string.Empty : reader["CountryName"].ToString();
            employeeModel.IsEdit = false;
            employeeModel.IsNew = false;
            employeeModel.IsDelete = false;
            return employeeModel;
        }
        #endregion
    }
}
