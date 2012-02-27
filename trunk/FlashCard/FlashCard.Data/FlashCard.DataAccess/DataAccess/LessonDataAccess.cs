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
        public IList<LessonModel> GetAll()
        {
            List<LessonModel> list = new List<LessonModel>();
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    SQLiteCommand myCommand = new SQLiteCommand(sqlConnect);
                    myCommand.CommandText = "select LessonID,LessonName,Cate, Cate.CategoryID as CategoryID, From Lesson as le "
                                             + " inner join Categories as Cate on Lesson.CategoryID == Categories.CategoryID";
                    SQLiteDataReader reader = myCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        LessonModel lessonModel = new LessonModel();
                        lessonModel.LessonID = (int)reader["LessonID"];
                        lessonModel.LessonName = reader["LessonName"].ToString();
                        lessonModel.CategoryID = (int)reader["CategoryID"];
                        lessonModel.CategoryModel = (CategoryModel)reader["Cate"];
                        list.Add(lessonModel);
                    }
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
                throw;
            }
            return list;
        }

        #endregion
    }
}
