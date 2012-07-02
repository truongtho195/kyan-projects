using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using FlashCard.Model;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using MVVMHelper.Common;
using log4net;


namespace FlashCard.DataAccess
{
    public class LessonDataAccess : DataAccessBase
    {
        #region Contructors
        public LessonDataAccess()
        {

        }
        #endregion

        #region Variable
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Properties


        #endregion

        #region Methods
        public LessonModel Get(int lessonID)
        {
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            LessonModel lessonModel = new LessonModel();

            string sql = "select * From Lessons where LessonID == @lessonID";
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    sqlConnect.Open();
                    sqlCommand = new SQLiteCommand(sqlConnect);
                    sqlCommand.CommandText = sql;
                    param = new SQLiteParameter("@lessonID", lessonID);
                    sqlCommand.Parameters.Add(param);
                    reader = sqlCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        lessonModel = MappingToModel(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
                throw;
            }
            finally
            {
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return lessonModel;
        }

        public LessonModel GetItem(int lessonID)
        {
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            LessonModel lessonModel = new LessonModel();
            BackSideDataAccess backSideDA = new BackSideDataAccess();
            string sql = "select * From Lessons where LessonID == @lessonID";
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
                    lessonModel = MappingToModel(reader);
                    var backSideModel = new BackSideModel() { BackSideID = -1 };
                    backSideModel.LessonID = lessonModel.LessonID;
                    //BackSideCollection
                    lessonModel.BackSideCollection = new ObservableCollection<BackSideModel>(backSideDA.GetAll(backSideModel));
                    switch (lessonModel.TypeModel.TypeOf)
                    {
                        case 1:
                            if (lessonModel.BackSideCollection != null && lessonModel.BackSideCollection.Count > 0)
                                lessonModel.BackSideModel = lessonModel.BackSideCollection.FirstOrDefault();
                            else
                                lessonModel.BackSideModel = new BackSideModel();
                            break;
                    }
                    lessonModel.ResetModelBase();
                }
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
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
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql1 = "select * from Lessons";
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    sqlConnect.Open();
                    sqlCommand = new SQLiteCommand(sqlConnect);
                    sqlCommand.CommandText = sql1;
                    reader = sqlCommand.ExecuteReader();
                    LessonModel lessonModel;
                    while (reader.Read())
                    {
                        lessonModel = MappingToModel(reader);
                        list.Add(lessonModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
                throw;
            }
            finally
            {
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return list;
        }



        public IList<LessonModel> GetAll(LessonModel lesson)
        {
            List<LessonModel> list = new List<LessonModel>();
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql = "select * from Lessons ";
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    sqlConnect.Open();
                    sqlCommand = new SQLiteCommand(sqlConnect);
                    string sqlcondition = string.Empty;
                    if (lesson.CategoryModel.CategoryID > -1)
                    {
                        if (string.IsNullOrWhiteSpace(sqlcondition))
                            sqlcondition += "where CategoryID==@categoryID";
                        else
                            sqlcondition += "&& CategoryID==@categoryID";
                        SQLiteParameter param = new SQLiteParameter("@categoryID", lesson.CategoryModel.CategoryID);
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
                        lessonModel = MappingToModel(reader);
                        list.Add(lessonModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
                throw;
            }
            finally
            {
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return list;
        }


        public List<LessonModel> GetAllWithRelation()
        {
            List<LessonModel> list = new List<LessonModel>();
            
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;

            BackSideDataAccess backSideDA = new BackSideDataAccess();
            string sql = "select * from Lessons ";
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    sqlConnect.Open();
                    sqlCommand = new SQLiteCommand(sqlConnect);
                    sqlCommand.CommandText = sql;
                    reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        LessonModel lessonModel = MappingToModel(reader);

                        var backSideModel = new BackSideModel() { BackSideID = -1 };
                        backSideModel.LessonID = lessonModel.LessonID;
                        //BackSideCollection
                        lessonModel.BackSideCollection = new ObservableCollection<BackSideModel>(backSideDA.GetAll(backSideModel));
                        switch (lessonModel.TypeModel.TypeOf)
                        {
                            case 1:
                                if (lessonModel.BackSideCollection != null && lessonModel.BackSideCollection.Count > 0)
                                    lessonModel.BackSideModel = lessonModel.BackSideCollection.FirstOrDefault();
                                else
                                    lessonModel.BackSideModel = new BackSideModel();
                                break;
                        }
                        lessonModel.ResetModelBase();
                        list.Add(lessonModel);
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
                throw;
            }
            finally
            {
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return list;
        }

        public IList<LessonModel> GetAllWithRelation(int lessonID)
        {
            List<LessonModel> list = new List<LessonModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            CategoryDataAccess categoryDA = new CategoryDataAccess();
            TypeDataAccess typeDA = new TypeDataAccess();
            BackSideDataAccess backSideDA = new BackSideDataAccess();
            string sql = "select * from Lessons where LessonID==@lessonID";
            try
            {
                //Categories
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                param = new SQLiteParameter("@lessonID", lessonID);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    LessonModel lessonModel = MappingToModel(reader);

                    var backSideModel = new BackSideModel() { BackSideID = -1 };
                    backSideModel.LessonID = lessonModel.LessonID;
                    //BackSideCollection
                    lessonModel.BackSideCollection = new ObservableCollection<BackSideModel>(backSideDA.GetAll(backSideModel));
                    lessonModel.IsEdit = false;
                    lessonModel.IsNew = false;
                    lessonModel.IsDelete = false;
                    list.Add(lessonModel);
                }

            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
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


        public bool Insert(LessonModel lessonModel)
        {
            bool result = false;
            string sql = "insert into Lessons (LessonName,Description,TypeID,CategoryID,IsActived) values (@LessonName,@Description,@TypeID,@CategoryID,@IsActived)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                CheckExitsCategory(lessonModel);
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                MappingToEntity(lessonModel, sqlCommand);
                sqlCommand.ExecuteNonQuery();
                BackSideDataAccess backSideDataAccess = new BackSideDataAccess();
                lessonModel.LessonID = (int)sqlConnect.LastInsertRowId;
                foreach (var item in lessonModel.BackSideCollection)
                {
                    item.LessonID = lessonModel.LessonID;
                    backSideDataAccess.Insert(item);
                }
                lessonModel.IsNew = false;
                lessonModel.IsDelete = false;
                lessonModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
            
                throw;
            }
            finally
            {
                sqlConnect.Dispose();
                sqlCommand.Dispose();
            
            }

            return result;
        }

        public bool Update(LessonModel lessonModel)
        {
            bool result = false;
            string sql = "update Lessons set LessonName=@LessonName,Description=@Description,TypeID=@TypeID,CategoryID=@CategoryID,IsActived=@IsActived where LessonID = @LessonID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                CheckExitsCategory(lessonModel);

                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                MappingToEntity(lessonModel, sqlCommand);
                sqlCommand.ExecuteNonQuery();
                BackSideDataAccess backSideDataAccess = new BackSideDataAccess();
                foreach (var item in lessonModel.BackSideCollection.ToList())
                {
                    item.LessonID = lessonModel.LessonID;
                    if (item.IsDelete)
                    {
                        backSideDataAccess.Delete(item);
                        lessonModel.BackSideCollection.Remove(item);
                    }
                    else if (item.IsNew)
                        backSideDataAccess.Insert(item);
                    else
                        backSideDataAccess.Update(item);

                }
                lessonModel.IsNew = false;
                lessonModel.IsDelete = false;
                lessonModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
                throw;
            }
            finally
            {
                sqlConnect.Dispose();
                sqlCommand.Dispose();
            }

            return result;
        }


        public bool Delete(LessonModel lessonModel)
        {
            bool result = false;
            string sql = "Delete From Lessons where (LessonID = @LessonID)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                
                sqlCommand = new SQLiteCommand(sqlConnect);
                SQLiteParameter param = new SQLiteParameter("@lessonID", lessonModel.LessonID);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(param);
                sqlCommand.ExecuteNonQuery();

                lessonModel.IsNew = false;
                lessonModel.IsDelete = false;
                lessonModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
                throw;
            }
            finally
            {
                sqlConnect.Dispose();
                sqlCommand.Dispose();
            }

            return result;
        }





        /// <summary>
        /// Insert if have a new category
        /// </summary>
        /// <param name="lessonModel"></param>
        private void CheckExitsCategory(LessonModel lessonModel)
        {
            if (lessonModel.CategoryModel.IsNew)
            {
                CategoryDataAccess catedataaccess = new CategoryDataAccess();
                catedataaccess.Insert(lessonModel.CategoryModel);
            }
        }

        #endregion

        #region Extend Methods
        private LessonModel MappingToModel(SQLiteDataReader reader)
        {
            LessonModel lessonModel;
            CategoryDataAccess categoryDA = new CategoryDataAccess();
            TypeDataAccess typeDA = new TypeDataAccess();
            lessonModel = new LessonModel();
            lessonModel.LessonID = int.Parse(reader["LessonID"].ToString());
            lessonModel.LessonName = reader["LessonName"].ToString();
            lessonModel.Description = FlowDocumentConverter.ConvertXMLToFlowDocument(reader["Description"].ToString());
            lessonModel.TypeID = int.Parse(reader["TypeID"].ToString());
            lessonModel.IsActived = bool.Parse(reader["IsActived"].ToString());
            //CategoryModel
            lessonModel.CategoryModel = categoryDA.Get(int.Parse(reader["CategoryID"].ToString()));
            //TypeMode
            lessonModel.TypeModel = typeDA.Get(lessonModel.TypeID);
            lessonModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
            lessonModel.IsDelete = false;
            lessonModel.IsEdit = false;
            lessonModel.IsNew = false;
            return lessonModel;
        }


        private void MappingToEntity(LessonModel lessonModel, SQLiteCommand sqlCommand)
        {
            sqlCommand.Parameters.Add(new SQLiteParameter("@LessonName", lessonModel.LessonName));
            sqlCommand.Parameters.Add(new SQLiteParameter("@Description", FlowDocumentConverter.ConvertFlowDocumentToSUBStringFormat(lessonModel.Description)));
            sqlCommand.Parameters.Add(new SQLiteParameter("@TypeID", lessonModel.TypeModel.TypeID));
            sqlCommand.Parameters.Add(new SQLiteParameter("@CategoryID", lessonModel.CategoryModel.CategoryID));
            sqlCommand.Parameters.Add(new SQLiteParameter("@LessonID", lessonModel.LessonID));
            sqlCommand.Parameters.Add(new SQLiteParameter("@IsActived", lessonModel.IsActived));
        }
        #endregion
    }
}
