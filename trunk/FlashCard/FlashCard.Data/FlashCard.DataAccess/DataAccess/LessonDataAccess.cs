using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlashCard.Model;
using System.Data.SQLite;
using Anito.Data;


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

        private ISession m_dataSession = null;

        private ISession DataSession
        {
            get
            {
                if (m_dataSession == null)
                    m_dataSession = ProviderFactory.GetSession();
                return m_dataSession;
            }
        }
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
                    myCommand.CommandText = "select * From Lesson "
                                             + " inner join Categories on Lesson.CategoryID == Categories.CategoryID"
                                            + " inner join Types on Types.TypeID == Categories.TypeID ";
                    SQLiteDataReader reader = myCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        LessonModel lessonModel = new LessonModel();
                        lessonModel.LessonID = (int)reader["LessonID"];
                        lessonModel.LessonName = reader["LessonName"].ToString();
                        lessonModel.CategoryID = (int)reader["CategoryID"];
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
