using System;
using System.Collections.Generic;
using System.Data.SQLite;
using FlashCard.Model;
using System.Collections.ObjectModel;
using log4net;

namespace FlashCard.DataAccess
{
    public class CategoryDataAccess : DataAccessBase
    {
        #region Contructors
        public CategoryDataAccess()
        {

        }
        #endregion

        #region Variables
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Properties


        #endregion

        #region Methods
        public CategoryModel Get(int categoryID)
        {
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteParameter param = null;
            SQLiteDataReader reader = null;
            CategoryModel categoryModel = new CategoryModel();
            string sql = "select * From Categories where CategoryID==@categoryID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@categoryID", categoryID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();


                if (reader.Read())
                {
                    categoryModel = MappingToModel(reader);
                    categoryModel.ResetModelBase();
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return categoryModel;
        }

        public IList<CategoryModel> GetAll()
        {
            List<CategoryModel> list = new List<CategoryModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql = "select * From Categories ";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
                CategoryModel categoryModel;
                while (reader.Read())
                {
                    categoryModel = MappingToModel(reader);
                    categoryModel.ResetModelBase();
                    list.Add(categoryModel);
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return list;
        }

        public IList<CategoryModel> GetAll(CategoryModel category)
        {
            List<CategoryModel> list = new List<CategoryModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            string sql = "select * from Categories";
            
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                string sqlcondition = string.Empty;
                if (category.CategoryID > -1)
                {
                    if (string.IsNullOrWhiteSpace(sqlcondition))
                        sqlcondition += "where CategoryID==@categoryID";
                    else
                        sqlcondition += "&& CategoryID==@categoryID";
                    param = new SQLiteParameter("@categoryID", category.CategoryID);
                    sqlCommand.Parameters.Add(param);
                }
                sqlCommand.CommandText = sql + sqlcondition;
                reader = sqlCommand.ExecuteReader();
                CategoryModel categoryModel;
                while (reader.Read())
                {
                    categoryModel = MappingToModel(reader);
                    categoryModel.ResetModelBase();
                    list.Add(categoryModel);
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
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return list;
        }

        public IList<CategoryModel> GetAllWithRelation()
        {
            List<CategoryModel> list = new List<CategoryModel>();
            SQLiteCommand sqlCommand= null;
            SQLiteDataReader reader=null;
            LessonDataAccess lessonDA = new LessonDataAccess();
            BackSideDataAccess backSideDA = new BackSideDataAccess();
            TypeDataAccess typeDA = new TypeDataAccess();
            string sqlCategories = "select * From Categories";
            try
            {
                
                //Categories
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    sqlConnect.Open();
                    sqlCommand = new SQLiteCommand(sqlConnect);
                    sqlCommand.CommandText = sqlCategories;
                    reader = sqlCommand.ExecuteReader();
                    //Initialize Lesson
                    while (reader.Read())
                    {
                        CategoryModel categoryModel = MappingToModel(reader);
                        //Lesson
                        LessonModel lesson = new LessonModel() { TypeID = -1, LessonID = -1 };
                        lesson.CategoryModel = categoryModel;
                        var lessonCollection = new List<LessonModel>();
                        foreach (var item in lessonDA.GetAll(lesson))
                        {
                            var backSideModel = new BackSideModel() { BackSideID = -1 };
                            backSideModel.LessonID = item.LessonID;
                            item.BackSideCollection = new ObservableCollection<BackSideModel>(backSideDA.GetAll(backSideModel));
                            item.TypeModel = typeDA.Get(item.TypeID);
                            lessonCollection.Add(item);
                        }
                        categoryModel.LessonCollection = lessonCollection;
                        categoryModel.ResetModelBase();
                        list.Add(categoryModel);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                throw;
            }
            finally
            {
                reader.Dispose();
                sqlCommand.Dispose();
            }
            return list;
        }

        public IList<CategoryModel> GetAllWithRelation(int categoryID)
        {
            List<CategoryModel> list = new List<CategoryModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand= null;
            SQLiteDataReader reader= null;
            SQLiteParameter param = null;
            LessonDataAccess lessonDA = new LessonDataAccess();
            BackSideDataAccess backSideDA = new BackSideDataAccess();
            TypeDataAccess typeDA = new TypeDataAccess();
            string sqlCategories = "select * From Categories where CategoryID==@categoryID";
            try
            {
                //Categories
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sqlCategories;
                param = new SQLiteParameter("@categoryID", categoryID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                //Initialize Lesson
                while (reader.Read())
                {
                    CategoryModel categoryModel= MappingToModel(reader);
                    
                    //Lesson
                    LessonModel lesson = new LessonModel() { TypeID = -1, LessonID = -1 };
                    var lessonCollection = new List<LessonModel>();
                    foreach (var item in lessonDA.GetAll(lesson))
                    {
                        var backSideModel = new BackSideModel() { BackSideID = -1 };
                        backSideModel.LessonID = item.LessonID;
                        item.BackSideCollection = new ObservableCollection<BackSideModel>(backSideDA.GetAll(backSideModel));
                        item.TypeModel = typeDA.Get(item.TypeID);
                        lessonCollection.Add(item);
                    }
                    categoryModel.LessonCollection = lessonCollection;
                    categoryModel.ResetModelBase();
                    list.Add(categoryModel);
                }
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
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

        public bool Insert(CategoryModel categoryModel)
        {
            bool result = false;
            string sql = "insert into Categories (CategoryName,Remark,IsActived) values (@CategoryName,@Remark,@IsActived)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                MappingToEntity(categoryModel, sqlCommand);
                sqlCommand.ExecuteNonQuery();
                categoryModel.CategoryID = (int)sqlConnect.LastInsertRowId;
                categoryModel.IsNew = false;
                categoryModel.IsDelete = false;
                categoryModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }

        public bool Update(CategoryModel categoryModel)
        {
            bool result = false;
            string sql = "update Categories set CategoryName=@CategoryName,Remark=@Remark,IsActived=@IsActived where CategoryID = @CategoryID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                MappingToEntity(categoryModel, sqlCommand);
                sqlCommand.Parameters.Add(new SQLiteParameter("@CategoryID", categoryModel.CategoryID));
                sqlCommand.ExecuteNonQuery();
                categoryModel.IsNew = false;
                categoryModel.IsDelete = false;
                categoryModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                log.Error(CatchException(ex));
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }

        public bool DeleteWithRelation(CategoryModel categoryModel)
        {
            bool result = false;
            string sql = "Delete from Categories WHERE (CategoryID = @CategoryID)";
            SQLiteCommand sqlCommand = null;
            SQLiteCommand sqlCommandLesson = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                //Lesson
                string sqlLesson = "Delete from Lessons WHERE (CategoryID = @CategoryID)";
                sqlCommandLesson = new SQLiteCommand(sqlConnect);
                sqlCommandLesson.CommandText = sqlLesson;
                sqlCommandLesson.Parameters.Add(new SQLiteParameter("@CategoryID", categoryModel.CategoryID));
                sqlCommandLesson.ExecuteNonQuery();

                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                MappingToEntity(categoryModel, sqlCommand);
                sqlCommand.Parameters.Add(new SQLiteParameter("@CategoryID", categoryModel.CategoryID));
                sqlCommand.ExecuteNonQuery();
                categoryModel.IsNew = false;
                categoryModel.IsDelete = false;
                categoryModel.IsEdit = false;
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                log.Error(CatchException(ex));
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                sqlCommandLesson.Dispose();
                throw;
            }
            return result;
        }
        
        #endregion

        #region Extend methods

        private void MappingToEntity(CategoryModel categoryModel, SQLiteCommand sqlCommand)
        {
            sqlCommand.Parameters.Add(new SQLiteParameter("@CategoryName", categoryModel.CategoryName));
            sqlCommand.Parameters.Add(new SQLiteParameter("@IsActived", categoryModel.IsActived));
            sqlCommand.Parameters.Add(new SQLiteParameter("@Remark", categoryModel.Remark));
        }
        private CategoryModel MappingToModel(SQLiteDataReader reader)
        {
            CategoryModel categoryModel = new CategoryModel();
            categoryModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
            categoryModel.CategoryName = reader["CategoryName"].ToString();
            categoryModel.Remark = reader["Remark"].ToString();
            categoryModel.IsActived = bool.Parse(reader["IsActived"].ToString());
            return categoryModel;
        }
        #endregion
    }
}
