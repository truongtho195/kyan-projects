﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FlashCard.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Markup;
using System.Windows.Documents;
using System.Xml;
using MVVMHelper.Common;

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
                    lessonModel = GetLessonModel(reader);
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
            string sql1 = "select * from Lessons";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql1;
                reader = sqlCommand.ExecuteReader();
                LessonModel lessonModel;
                while (reader.Read())
                {
                    lessonModel = GetLessonModel(reader);
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
            string sql = "select * from Lessons";


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
                    lessonModel = GetLessonModel(reader);
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
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            CategoryDataAccess categoryDA = new CategoryDataAccess();
            TypeDataAccess typeDA = new TypeDataAccess();
            BackSideDataAccess backSideDA = new BackSideDataAccess();
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
                    LessonModel lessonModel = GetLessonModel(reader);
                    //CategoryModel
                    lessonModel.CategoryModel = categoryDA.Get(lessonModel.CategoryID);
                    //TypeMode
                    lessonModel.TypeModel = typeDA.Get(lessonModel.TypeID);
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
                    lessonModel.IsNew = false;
                    lessonModel.IsEdit = false;
                    lessonModel.IsDelete = false;
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
                    LessonModel lessonModel = GetLessonModel(reader);
                    //CategoryModel
                    lessonModel.CategoryModel = categoryDA.Get(lessonModel.CategoryID);
                    //TypeMode
                    lessonModel.TypeModel = typeDA.Get(lessonModel.TypeID);
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


        public bool Insert(LessonModel lessonModel)
        {
            bool result = false;
            string sql = "insert into Lessons (LessonName,Description,TypeID,CategoryID) values (@LessonName,@Description,@TypeID,@CategoryID)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                Edit(lessonModel, sqlCommand);
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
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }

        public bool Update(LessonModel lessonModel)
        {
            bool result = false;
            string sql = "update Lessons set LessonName=@LessonName,Description=@Description,TypeID=@TypeID,CategoryID=@CategoryID where LessonID = @LessonID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                Edit(lessonModel, sqlCommand);
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
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }


        private LessonModel GetLessonModel(SQLiteDataReader reader)
        {
            LessonModel lessonModel;
            lessonModel = new LessonModel();
            lessonModel.LessonID = int.Parse(reader["LessonID"].ToString());
            lessonModel.LessonName = reader["LessonName"].ToString();
            lessonModel.Description = FlowDocumentConverter.ConvertXMLToFlowDocument(reader["Description"].ToString());
            lessonModel.TypeID = int.Parse(reader["TypeID"].ToString());
            lessonModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
            lessonModel.IsDelete = false;
            lessonModel.IsEdit = false;
            lessonModel.IsNew = false;
            return lessonModel;
        }


        private void Edit(LessonModel lessonModel, SQLiteCommand sqlCommand)
        {
            sqlCommand.Parameters.Add(new SQLiteParameter("@LessonName", lessonModel.LessonName));
            sqlCommand.Parameters.Add(new SQLiteParameter("@Description", FlowDocumentConverter.ConvertFlowDocumentToSUBStringFormat(lessonModel.Description)));
            sqlCommand.Parameters.Add(new SQLiteParameter("@TypeID", lessonModel.TypeID));
            sqlCommand.Parameters.Add(new SQLiteParameter("@CategoryID", lessonModel.CategoryID));
            sqlCommand.Parameters.Add(new SQLiteParameter("@LessonID", lessonModel.LessonID));
        }

      

        #endregion
    }
}