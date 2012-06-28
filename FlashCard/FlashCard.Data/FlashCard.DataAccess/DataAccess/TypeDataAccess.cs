using System;
using System.Collections.Generic;
using System.Data.SQLite;
using FlashCard.Model;
using System.Collections.ObjectModel;


namespace FlashCard.DataAccess
{
    public class TypeDataAccess : DataAccessBase
    {
        #region Contructors
        public TypeDataAccess()
        {

        }
        #endregion

        #region Properties


        #endregion

        #region Methods
        public TypeModel Get(int typeID)
        {
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteParameter param = null;
            SQLiteDataReader reader = null;
            TypeModel typeModel = new TypeModel();
            string sql = "select * from Types where TypeID==@typeID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@typeID", typeID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();

                if (reader.Read())
                {
                    typeModel = GetTypeModel(reader);
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return typeModel;
        }


        public IList<TypeModel> GetAll()
        {
            List<TypeModel> list = new List<TypeModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql = "select * From Types";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
                
                while (reader.Read())
                {
                    TypeModel typeModel = GetTypeModel(reader);
                    list.Add(typeModel);
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return list;             
        }

        public IList<TypeModel> GetAll(TypeModel type)
        {
            List<TypeModel> list = new List<TypeModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql = "select * from Types";

            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                string sqlcondition = string.Empty;
                if (type.TypeID > -1)
                {
                    if (string.IsNullOrWhiteSpace(sqlcondition))
                        sqlcondition += "where TypeID==@typeID";
                    else
                        sqlcondition += "&& TypeID==@typeID";
                    SQLiteParameter param = new SQLiteParameter("@typeID", type.TypeID);
                    sqlCommand.Parameters.Add(param);
                }
                sqlCommand.CommandText = sql + sqlcondition;
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    TypeModel typeModel = GetTypeModel(reader);
                    list.Add(typeModel);
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return list;
        }

      

        public IList<TypeModel> GetAllWithRelation()
        {
            List<TypeModel> list = new List<TypeModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            LessonDataAccess lessonDA = new LessonDataAccess();
            BackSideDataAccess backSideDA = new BackSideDataAccess();
            CategoryDataAccess categoryDA = new CategoryDataAccess();

            string sql = "select * from Types";

            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql ;
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    TypeModel typeModel = new TypeModel();
                    typeModel.TypeID = int.Parse(reader["TypeID"].ToString());
                    typeModel.Name = reader["Name"].ToString();
                    //Lesson
                    LessonModel lesson = new LessonModel() { LessonID = -1 };
                    lesson.TypeID = typeModel.TypeID;
                    var lessonCollection = new List<LessonModel>();
                    foreach (var item in lessonDA.GetAll(lesson))
                    {
                        var backSideModel = new BackSideModel() { BackSideID = -1 };
                        backSideModel.LessonID = item.LessonID;
                        item.BackSideCollection = new ObservableCollection<BackSideModel>(backSideDA.GetAll(backSideModel));
                        item.CategoryModel = categoryDA.Get(item.TypeID);
                        lessonCollection.Add(item);
                    }
                    typeModel.LessonCollection = lessonCollection;
                    list.Add(typeModel);
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return list;
        }

        public IList<TypeModel> GetAllWithRelation(int typeID)
        {
            List<TypeModel> list = new List<TypeModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            LessonDataAccess lessonDA = new LessonDataAccess();
            BackSideDataAccess backSideDA = new BackSideDataAccess();
            CategoryDataAccess categoryDA = new CategoryDataAccess();

            string sql = "select * from Types where TypeID==@typeID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@typeID", typeID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    TypeModel typeModel = GetTypeModel(reader);
                    //Lesson
                    LessonModel lesson = new LessonModel() {LessonID = -1 };
                    lesson.TypeID = typeModel.TypeID;
                    var lessonCollection = new List<LessonModel>();
                    foreach (var item in lessonDA.GetAll(lesson))
                    {
                        var backSideModel = new BackSideModel() { BackSideID = -1 };
                        backSideModel.LessonID = item.LessonID;
                        item.BackSideCollection = new ObservableCollection<BackSideModel>(backSideDA.GetAll(backSideModel));
                        item.CategoryModel = categoryDA.Get(item.TypeID);
                        lessonCollection.Add(item);
                    }
                    typeModel.LessonCollection = lessonCollection;
                    list.Add(typeModel);
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return list;
        }

        #endregion


        private TypeModel GetTypeModel(SQLiteDataReader reader)
        {
            TypeModel typeModel = new TypeModel();
            typeModel.TypeID = int.Parse(reader["TypeID"].ToString());
            typeModel.Name = reader["Name"].ToString();
            typeModel.TypeOf = int.Parse(reader["TypeOf"].ToString());
            typeModel.IsEdit = false;
            typeModel.IsNew = false;
            typeModel.IsDelete = false;
            return typeModel;
        }
    }
}
