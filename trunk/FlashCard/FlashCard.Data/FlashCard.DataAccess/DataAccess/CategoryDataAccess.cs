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
        public IList<CategoryModel> GetAll()
        {
            List<CategoryModel> list = new List<CategoryModel>();
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                SQLiteCommand myCommand = new SQLiteCommand(sqlConnect);
                myCommand.CommandText = "select * From Categories";
                SQLiteDataReader reader = myCommand.ExecuteReader();
                while (reader.Read())
                {
                    SQLiteCommand sqlLessonCmd = new SQLiteCommand(sqlConnect);
                    sqlLessonCmd.CommandText = "select * from Lessons where CategoryID ==@cateID";
                    CategoryModel categoryModel = new CategoryModel();
                    categoryModel.CategoryID = int.Parse(reader["CategoryID"].ToString());
                    categoryModel.CategoryName = reader["CategoryName"].ToString();

                    SQLiteParameter param = new SQLiteParameter("@cateID", categoryModel.CategoryID);
                    sqlLessonCmd.Parameters.Add(param);
                    SQLiteDataReader reader1 = sqlLessonCmd.ExecuteReader();

                    categoryModel.LessonCollection = new List<LessonModel>();
                    while (reader1.Read())
                    {
                        LessonModel lessonModel = new LessonModel();
                        lessonModel.CategoryID = int.Parse(reader1["CategoryID"].ToString());
                        lessonModel.LessonID = int.Parse(reader1["LessonID"].ToString());
                        lessonModel.TypeID = int.Parse(reader1["TypeID"].ToString());
                        lessonModel.CategoryModel = categoryModel;
                        lessonModel.LessonName = reader1["LessonName"].ToString();
                        categoryModel.LessonCollection.Add(lessonModel);
                    }
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
