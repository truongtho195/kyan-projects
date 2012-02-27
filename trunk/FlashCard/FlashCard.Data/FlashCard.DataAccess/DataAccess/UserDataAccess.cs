using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using FlashCard.Model;

namespace FlashCard.DataAccess
{
    public class UserDataAccess : DataAccessBase
    {
        #region Contructors
        public UserDataAccess()
        {

        }
        #endregion

        #region Methods
        public IList<UserModel> GetAll()
        {
            List<UserModel> list = new List<UserModel>();
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    SQLiteCommand myCommand = new SQLiteCommand(sqlConnect);
                    myCommand.CommandText = "select * From Users";
                    SQLiteDataReader reader = myCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        UserModel userModel = new UserModel();
                        userModel.UserID = (int)reader["UserID"];
                        userModel.UserName = reader["UserName"].ToString();
                        userModel.FullName = reader["FullName"].ToString();
                        userModel.Password = reader["Password"].ToString();
                        list.Add(userModel);
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
