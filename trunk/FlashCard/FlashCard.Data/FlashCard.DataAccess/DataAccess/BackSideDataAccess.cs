using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FlashCard.Model;


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
        public IList<BackSideModel> GetAll()
        {
            List<BackSideModel> list = new List<BackSideModel>();
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    SQLiteCommand myCommand = new SQLiteCommand(sqlConnect);
                    myCommand.CommandText = "select * From BackSide";
                    SQLiteDataReader reader = myCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        BackSideModel backSideModel = new BackSideModel();
                        backSideModel.BackSideID = (int)reader["BackSideID"];
                        backSideModel.LessonID = (int)reader["LessonID"];
                        backSideModel.Content = reader["Content"].ToString();
                        list.Add(backSideModel);
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
