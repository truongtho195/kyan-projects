using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using FlashCard.Model;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using MVVMHelper.Common;
using log4net;


namespace FlashCard.DataAccess
{
    public class SetupDataAccess : DataAccessBase
    {
        #region Contructors
        public SetupDataAccess()
        {

        }
        #endregion

        #region Variable
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Properties


        #endregion

        #region Methods
        public SetupModel Get(int setupID)
        {
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            SetupModel setupModel = new SetupModel();

            string sql = "select * From Setups where SetupID == @SetupID";
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    sqlConnect.Open();
                    sqlCommand = new SQLiteCommand(sqlConnect);
                    sqlCommand.CommandText = sql;
                    param = new SQLiteParameter("@SetupID", setupID);
                    sqlCommand.Parameters.Add(param);
                    reader = sqlCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        setupModel = MappingToModel(reader);
                    }
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
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return setupModel;
        }


        public IList<SetupModel> GetAll()
        {
            List<SetupModel> list = new List<SetupModel>();
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            string sql1 = "select * from Setups ";
            try
            {
                using (SQLiteConnection sqlConnect = new SQLiteConnection(ConnectionString))
                {
                    sqlConnect.Open();
                    sqlCommand = new SQLiteCommand(sqlConnect);
                    sqlCommand.CommandText = sql1;
                    reader = sqlCommand.ExecuteReader();
                    SetupModel setupModel;
                    while (reader.Read())
                    {
                        setupModel = MappingToModel(reader);
                        list.Add(setupModel);
                    }
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
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return list;
        }


        public bool Insert(SetupModel setupModel)
        {

            bool result = false;
            string sql = "insert into Setups(ViewTimeSecond,DistanceTimeSecond,IsLimitCard,LimitCardNum,IsEnableSlideShow,IsEnableLoop,IsEnableSoundForShow) values (@ViewTimeSecond,@DistanceTimeSecond,@IsLimitCard,@LimitCardNum,@IsEnableSlideShow,@IsEnableLoop,@IsEnableSoundForShow)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                MappingToEntity(setupModel, sqlCommand);
                sqlCommand.ExecuteNonQuery();
                setupModel.SetupID = (int)sqlConnect.LastInsertRowId;
                setupModel.ResetModelBase();
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
                sqlConnect.Dispose();
                sqlCommand.Dispose();

            }

            return result;
        }

        public bool Update(SetupModel setupModel)
        {
            bool result = false;
            string sql = "update Setups set ViewTimeSecond=@ViewTimeSecond,DistanceTimeSecond=@DistanceTimeSecond,IsLimitCard=@IsLimitCard,LimitCardNum=@LimitCardNum,IsEnableSlideShow=@IsEnableSlideShow, IsEnableLoop = @IsEnableLoop,IsEnableSoundForShow=@IsEnableSoundForShow where SetupID = @SetupID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                MappingToEntity(setupModel, sqlCommand);
                sqlCommand.ExecuteNonQuery();
                setupModel.ResetModelBase();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                log.Error(CatchException(ex));
                if (log.IsDebugEnabled)
                    System.Windows.MessageBox.Show(ex.ToString(), "Debug ! Error");
                throw;
            }
            finally
            {
                sqlConnect.Dispose();
                sqlConnect.Dispose();
                sqlCommand.Dispose();
            }

            return result;
        }


        public bool Delete(SetupModel setupModel)
        {
            bool result = false;
            string sql = "Delete From Setups where (SetupID = @SetupID)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                 sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();

                sqlCommand = new SQLiteCommand(sqlConnect);
                SQLiteParameter param = new SQLiteParameter("@SetupID", setupModel.SetupID);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(param);
                sqlCommand.ExecuteNonQuery();
                setupModel.ResetModelBase();
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
                sqlConnect.Dispose();
                sqlCommand.Dispose();
            }

            return result;
        }

        #endregion

        #region Extend Methods
        private SetupModel MappingToModel(SQLiteDataReader reader)
        {
            SetupModel setupModel = new SetupModel();
            setupModel.SetupID = int.Parse(reader["SetupID"].ToString());
            setupModel.ViewTimeSecond = int.Parse(reader["ViewTimeSecond"].ToString());
            setupModel.DistanceTimeSecond = int.Parse(reader["DistanceTimeSecond"].ToString());
            setupModel.IsLimitCard = bool.Parse(reader["IsLimitCard"].ToString());
            setupModel.LimitCardNum = int.Parse(reader["LimitCardNum"].ToString());
            setupModel.IsEnableSlideShow = bool.Parse(reader["IsEnableSlideShow"].ToString());
            setupModel.IsEnableLoop = bool.Parse(reader["IsEnableLoop"].ToString());
            setupModel.IsEnableSlideShow = bool.Parse(reader["IsEnableSoundForShow"] == System.DBNull.Value ?"false" : reader["IsEnableSoundForShow"].ToString());
            setupModel.ResetModelBase();
            return setupModel;
        }


        private void MappingToEntity(SetupModel setupModel, SQLiteCommand sqlCommand)
        {
            if(!setupModel.IsNew)
                sqlCommand.Parameters.Add(new SQLiteParameter("@SetupID", setupModel.SetupID));
            sqlCommand.Parameters.Add(new SQLiteParameter("@ViewTimeSecond", setupModel.ViewTimeSecond));
            sqlCommand.Parameters.Add(new SQLiteParameter("@DistanceTimeSecond", setupModel.DistanceTimeSecond));
            sqlCommand.Parameters.Add(new SQLiteParameter("@IsLimitCard", setupModel.IsLimitCard));
            sqlCommand.Parameters.Add(new SQLiteParameter("@LimitCardNum", setupModel.LimitCardNum));
            sqlCommand.Parameters.Add(new SQLiteParameter("@IsEnableSlideShow", setupModel.IsEnableSlideShow));
            sqlCommand.Parameters.Add(new SQLiteParameter("@IsEnableLoop", setupModel.IsEnableLoop));
            sqlCommand.Parameters.Add(new SQLiteParameter("@IsEnableSoundForShow", setupModel.IsEnableLoop));
        }
        #endregion
    }
}
