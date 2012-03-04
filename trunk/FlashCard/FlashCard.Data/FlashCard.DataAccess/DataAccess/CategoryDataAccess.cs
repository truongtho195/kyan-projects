using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FlashCard.Model;

namespace FlashCard.DataAccess
{
    public class CategoryDataAccess : DataAccessBase
    {
        #region Contructors
        public CategoryDataAccess()
        {

        }
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
                    categoryModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
                    categoryModel.CategoryName = reader["CategoryName"].ToString();
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
                    categoryModel = new CategoryModel();
                    categoryModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
                    categoryModel.CategoryName = reader["CategoryName"].ToString();
                    list.Add(categoryModel);
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

        public IList<CategoryModel> GetAll(CategoryModel category)
        {
            List<CategoryModel> list = new List<CategoryModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
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
                    SQLiteParameter param = new SQLiteParameter("@categoryID", category.CategoryID);
                    sqlCommand.Parameters.Add(param);
                }
                sqlCommand.CommandText = sql + sqlcondition;
                reader = sqlCommand.ExecuteReader();
                CategoryModel categoryModel;
                while (reader.Read())
                {
                    categoryModel = new CategoryModel();
                    categoryModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
                    categoryModel.CategoryName = reader["CategoryName"].ToString();
                    list.Add(categoryModel);
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

        public IList<CategoryModel> GetAllWithRelation()
        {
            List<CategoryModel> list = new List<CategoryModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand;
            SQLiteDataReader reader;
            string sqlCategories = "select * From Categories";
            try
            {
                //Categories
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sqlCategories;
                reader = sqlCommand.ExecuteReader();

                //Initialize Lesson
                while (reader.Read())
                {
                    CategoryModel categoryModel = new CategoryModel();
                    categoryModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
                    categoryModel.CategoryName = reader["CategoryName"].ToString();

                    //Lesson
                    LessonDataAccess lessonDA = new LessonDataAccess();
                    LessonModel lesson = new LessonModel() { TypeID=-1,LessonID=-1};
                    lesson.CategoryID = categoryModel.CategoryID;
                    categoryModel.LessonCollection= new List<LessonModel>(lessonDA.GetAll(lesson));
                    list.Add(categoryModel);
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
