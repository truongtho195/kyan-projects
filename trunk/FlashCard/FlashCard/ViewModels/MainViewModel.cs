using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;


namespace FlashCard.ViewModels
{
    class MainViewModel
    {
        string sqlConnection;
        public MainViewModel()
        {
            sqlConnection = "Data Source=SmartFlashCardDB";
            DataTable dt = new DataTable();
           
            try
            {

                SQLiteConnection cnn = new SQLiteConnection(sqlConnection);
                cnn.Open();

                SQLiteCommand mycommand = new SQLiteCommand(cnn);

                mycommand.CommandText = "select * from users";

                SQLiteDataReader reader = mycommand.ExecuteReader();

                dt.Load(reader);

                reader.Close();

                cnn.Close();

            }

            catch (Exception e)
            {

                throw new Exception(e.Message);

            }



        }


    }
}
