using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FlashCard.Model;
using MVVMHelper.Common;


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
                    backSideModel = GetBackSideModel(reader);
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

                while (reader.Read())
                {
                    BackSideModel backSideModel = GetBackSideModel(reader);
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
                    SQLiteParameter param = new SQLiteParameter("@backSideID", backSide.BackSideID);
                    sqlCommand.Parameters.Add(param);
                }
                if (backSide.LessonID > -1)
                {
                    if (string.IsNullOrWhiteSpace(sqlcondition))
                        sqlcondition += "where LessonID==@lessonID";
                    else
                        sqlcondition += "&& LessonID==@lessonID";
                    SQLiteParameter param = new SQLiteParameter("@lessonID", backSide.LessonID);
                    sqlCommand.Parameters.Add(param);
                }
                sqlCommand.CommandText = sql + sqlcondition;
                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    BackSideModel backSideModel = GetBackSideModel(reader);
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
                while (reader.Read())
                {
                    //BackSideModel
                    BackSideModel backSideModel = GetBackSideModel(reader);
                    //LessonModel
                    backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
                    //CategoryModel
                    backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
                    //TypeModel
                    backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
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
                    backSideModel = GetBackSideModel(reader);
                    //LessonModel
                    backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
                    //CategoryModel
                    backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
                    //TypeModel
                    backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
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


        public bool Insert(BackSideModel backSideModel)
        {
            bool result = false;
            string sql = "insert into BackSide (LessonID,Content,IsCorrect) values (@LessonID,@Content,@IsCorrect)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@LessonID", backSideModel.LessonID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Content",FlowDocumentConverter.ConvertFlowDocumentToSUBStringFormat(backSideModel.BackSideDetail)));
                sqlCommand.Parameters.Add(new SQLiteParameter("@IsCorrect", backSideModel.IsCorrect));
                sqlCommand.ExecuteNonQuery();
                backSideModel.BackSideID = (int)sqlConnect.LastInsertRowId;
                backSideModel.IsNew = false;
                backSideModel.IsDelete = false;
                backSideModel.IsEdit = false;
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


        public bool Update(BackSideModel backSideModel)
        {
            bool result = false;
            string sql = "Update BackSide set LessonID=@LessonID,Content=@Content,IsCorrect=@IsCorrect where BackSideID = @BackSideID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@LessonID", backSideModel.LessonID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Content",FlowDocumentConverter.ConvertFlowDocumentToSUBStringFormat(backSideModel.BackSideDetail)));
                var correct = backSideModel.IsCorrect.HasValue ? backSideModel.IsCorrect : false;
                sqlCommand.Parameters.Add(new SQLiteParameter("@IsCorrect", correct));
                sqlCommand.Parameters.Add(new SQLiteParameter("@BackSideID", backSideModel.BackSideID));
                sqlCommand.ExecuteNonQuery();
                backSideModel.IsNew = false;
                backSideModel.IsDelete = false;
                backSideModel.IsEdit = false;
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

        public bool Delete(BackSideModel backSideModel)
        {
            bool result = false;
            string sql = "Delete BackSide where BackSideID = @BackSideID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@BackSideID", backSideModel.BackSideID));
                sqlCommand.ExecuteNonQuery();
                backSideModel.IsNew = false;
                backSideModel.IsDelete = false;
                backSideModel.IsEdit = false;
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


        private BackSideModel GetBackSideModel(SQLiteDataReader reader)
        {
            BackSideModel backSideModel = new BackSideModel();
            backSideModel.BackSideID = int.Parse(reader["BackSideID"].ToString());
            backSideModel.LessonID = int.Parse(reader["LessonID"].ToString());
            backSideModel.BackSideDetail = FlowDocumentConverter.ConvertXMLToFlowDocument(reader["Content"].ToString());
            var isCorrect = reader["IsCorrect"];
            if (isCorrect != System.DBNull.Value)
                backSideModel.IsCorrect = bool.Parse(reader["IsCorrect"].ToString());

            backSideModel.IsEdit = false;
            backSideModel.IsNew = false;
            backSideModel.IsDelete = false;
            return backSideModel;
        }
        #endregion
    }
}
