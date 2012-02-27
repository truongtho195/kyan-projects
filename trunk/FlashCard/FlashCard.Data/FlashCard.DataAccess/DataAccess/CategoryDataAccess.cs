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
                myCommand.CommandText = "select Categories.CategoryID,CategoryName,Lessons  as lesson From Categories inner join Lessons on Categories.CategoryID = Lessons.CategoryID";
                SQLiteDataReader reader = myCommand.ExecuteReader();
                while (reader.Read())
                {
                    CategoryModel categoryModel = new CategoryModel();
                    categoryModel.CategoryID = (int)reader["CategoryID"];
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
               
            }
            return list;
        }

        #endregion
    }
}
