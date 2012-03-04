using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FlashCard.Model;

namespace FlashCard.DataAccess
{
    public class LessonDataAccess : DataAccessBase
    {
        #region Contructors
        public LessonDataAccess()
        {

        }
        #endregion

        #region Properties


        #endregion

        #region Methods
        public LessonModel Get(int lessonID)
        {
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            LessonModel lessonModel = new LessonModel();

            string sql = "select * From Lesson where LessonID == @lessonID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@lessonID", lessonID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    lessonModel.LessonID = (int)reader["LessonID"];
                    lessonModel.LessonName = reader["LessonName"].ToString();
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
            return lessonModel;
        }

        public IList<LessonModel> GetAll()
        {
            List<LessonModel> list = new List<LessonModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql = "select * From Lesson";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
                LessonModel lessonModel;
                while (reader.Read())
                {
                    lessonModel = new LessonModel();
                    lessonModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    lessonModel.LessonName = reader["LessonName"].ToString();
                    lessonModel.TypeID = int.Parse(reader["TypeID"].ToString());
                    list.Add(lessonModel);
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

        public IList<LessonModel> GetAll(LessonModel lesson)
        {
            List<LessonModel> list = new List<LessonModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            //SQLiteParameter param = null;
            string sql = "select * from Lessons ";


            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                string sqlcondition = string.Empty;
                if (lesson.CategoryID > -1)
                {
                    if (string.IsNullOrWhiteSpace(sqlcondition))
                        sqlcondition += "where CategoryID==@categoryID";
                    else
                        sqlcondition += "&& CategoryID==@categoryID";
                    SQLiteParameter param = new SQLiteParameter("@categoryID", lesson.CategoryID);
                    sqlCommand.Parameters.Add(param);
                }
                if (lesson.LessonID > -1)
                {
                    if (string.IsNullOrWhiteSpace(sqlcondition))
                        sqlcondition += "where LessonID == @lessonID";
                    else
                        sqlcondition += "&& LessonID == @lessonID";

                    SQLiteParameter param = new SQLiteParameter("@lessonID", lesson.LessonID);
                    sqlCommand.Parameters.Add(param);
                }
                if (lesson.TypeID > -1)
                {
                    if (string.IsNullOrWhiteSpace(sqlcondition))
                        sqlcondition += "where TypeID == @typeID";
                    else
                        sqlcondition += "&& TypeID == @typeID";
                    SQLiteParameter param = new SQLiteParameter("@typeID", lesson.TypeID);

                    sqlCommand.Parameters.Add(param);
                }
                sqlCommand.CommandText = sql + sqlcondition;
                reader = sqlCommand.ExecuteReader();
                LessonModel lessonModel;
                while (reader.Read())
                {
                    lessonModel = new LessonModel();
                    lessonModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    lessonModel.LessonName = reader["LessonName"].ToString();
                    lessonModel.TypeID = int.Parse(reader["TypeID"].ToString());
                    list.Add(lessonModel);
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


        public IList<LessonModel> GetAllWithRelation()
        {
            List<LessonModel> list = new List<LessonModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand;
            SQLiteDataReader reader;
            string sql = "select * from Lessons";
            try
            {
                //Categories
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    CategoryDataAccess categoryDA = new CategoryDataAccess();
                    
                    LessonModel lessonModel = new LessonModel();

                    lessonModel.LessonID = int.Parse(reader["LessonID"].ToString());
                    lessonModel.LessonName = reader["LessonName"].ToString();
                    lessonModel.TypeID = int.Parse(reader["TypeID"].ToString());
                    lessonModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
                    lessonModel.CategoryModel = categoryDA.Get(lessonModel.CategoryID);
                    //lessonModel.TypeModel
                    
                    list.Add(lessonModel);
                }

            }
            catch (Exception ex)
            {
                CatchException(ex);
                throw;
            }
            finally
            {

            }
            return list;
        }


        #endregion
    }
}
