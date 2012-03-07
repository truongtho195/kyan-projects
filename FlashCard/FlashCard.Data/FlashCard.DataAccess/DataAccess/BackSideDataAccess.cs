using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FlashCard.Model;


namespace FlashCard.DataAccess
{
    public class BackSideDataAccess : DataAccessBase
    {
        #region Contructors
        public BackSideDataAccess()
        {

        }
        #endregion

        #region Properties


        #endregion

        #region Methods
        public BackSideModel Get(int backSideID)
        {
            BackSideModel backSideModel = new BackSideModel();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            string sql = "select * From BackSide where BackSideID ==@backSideID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@backSideID", backSideID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    backSideModel.BackSideID = int.Parse(reader["BackSideID"].ToString());
                    backSideModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    backSideModel.Content = reader["Content"].ToString();
                    backSideModel.IsCorrect = bool.Parse(reader["IsCorrect"].ToString());
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
            return backSideModel;
        }

        public IList<BackSideModel> GetAll()
        {
            List<BackSideModel> list = new List<BackSideModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
           
            SQLiteDataReader reader = null;
            string sql = "select * From BackSide";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                
                reader = sqlCommand.ExecuteReader();
                BackSideModel backSideModel;
                while (reader.Read())
                {
                    backSideModel = new BackSideModel();
                    backSideModel.BackSideID = int.Parse(reader["BackSideID"].ToString());
                    backSideModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    backSideModel.Content = reader["Content"].ToString();
                    backSideModel.IsCorrect = bool.Parse(reader["IsCorrect"].ToString());
                    list.Add(backSideModel);
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

        public IList<BackSideModel> GetAll(BackSideModel backSide)
        {
            List<BackSideModel> list = new List<BackSideModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql = "select * from BackSide ";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                string sqlcondition = string.Empty;
                if (backSide.BackSideID > -1)
                {
                    if (string.IsNullOrWhiteSpace(sqlcondition))
                        sqlcondition += "where BackSideID==@backSideID";
                    else
                        sqlcondition += "&& BackSideID==@backSideID";
                    SQLiteParameter param = new SQLiteParameter("@categoryID", backSide.BackSideID);
                    sqlCommand.Parameters.Add(param);
                }
                sqlCommand.CommandText = sql + sqlcondition;
                reader = sqlCommand.ExecuteReader();
                BackSideModel backSideModel;
                while (reader.Read())
                {
                    backSideModel = new BackSideModel();
                    backSideModel.BackSideID = int.Parse(reader["BackSideID"].ToString());
                    backSideModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    backSideModel.Content = reader["Content"].ToString();
                    backSideModel.IsCorrect = bool.Parse(reader["IsCorrect"].ToString());
                    list.Add(backSideModel);
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


        public IList<BackSideModel> GetAllWithRelation()
        {
            List<BackSideModel> list = new List<BackSideModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            LessonDataAccess lessonDA = new LessonDataAccess();
            CategoryDataAccess categoryDA = new CategoryDataAccess();
            TypeDataAccess typeDA = new TypeDataAccess();
            string sql = "select * from BackSide ";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
                BackSideModel backSideModel;
                while (reader.Read())
                {
                    //BackSideModel
                    backSideModel = new BackSideModel();
                    backSideModel.BackSideID = int.Parse(reader["BackSideID"].ToString());
                    backSideModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    backSideModel.Content = reader["Content"].ToString();
                    backSideModel.IsCorrect = bool.Parse(reader["IsCorrect"].ToString());
                    //LessonModel
                    backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
                    //CategoryModel
                    backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryID);
                    //TypeModel
                    backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryID);
                    list.Add(backSideModel);
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

        public IList<BackSideModel> GetAllWithRelation(int backSideID)
        {
            List<BackSideModel> list = new List<BackSideModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            LessonDataAccess lessonDA = new LessonDataAccess();
            CategoryDataAccess categoryDA = new CategoryDataAccess();
            TypeDataAccess typeDA = new TypeDataAccess();
            string sql = "select * from BackSide where BackSideID==@backSideID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@backSideID", backSideID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                BackSideModel backSideModel;
                while (reader.Read())
                {
                    //BackSideModel
                    backSideModel = new BackSideModel();
                    backSideModel.BackSideID = int.Parse(reader["BackSideID"].ToString());
                    backSideModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    backSideModel.Content = reader["Content"].ToString();
                    backSideModel.IsCorrect = bool.Parse(reader["IsCorrect"].ToString());
                    //LessonModel
                    backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
                    //CategoryModel
                    backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryID);
                    //TypeModel
                    backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryID);
                    list.Add(backSideModel);
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

        #endregion
    }
}
