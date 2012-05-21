using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DemoFalcon.Model;

namespace DemoFalcon.DataAccess
{
    public class DepartmentDetailDataAccess : DataAccessBase
    {
        #region Contructors
        public DepartmentDetailDataAccess()
        {
        }
        #endregion

        #region Methods
        public DepartmentDetailModel Get(int departmentDetailID)
        {
            DepartmentDetailModel departmentDetailModel = new DepartmentDetailModel();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            string sql = "select * From DepartmentDetails where DetailID ==@DetailID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@DetailID", departmentDetailID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    departmentDetailModel = GetDepartmentDetailModel(reader);
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
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return departmentDetailModel;
        }

        public IList<DepartmentDetailModel> GetAll()
        {
            List<DepartmentDetailModel> list = new List<DepartmentDetailModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;

            SQLiteDataReader reader = null;
            string sql = "select * From DepartmentDetails";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;

                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    DepartmentDetailModel departmentDetailModel = GetDepartmentDetailModel(reader);
                    list.Add(departmentDetailModel);
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
                sqlCommand.Dispose();
                reader.Dispose();
            }
            return list;
        }

        //public IList<DepartmentDetailModel> GetAll(DepartmentDetailModel backSide)
        //{
        //    List<DepartmentDetailModel> list = new List<DepartmentDetailModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    string sql = "select * from departmentDetailModel ";

        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        string sqlcondition = string.Empty;
        //        if (backSide.DetailID > -1)
        //        {
        //            if (string.IsNullOrWhiteSpace(sqlcondition))
        //                sqlcondition += "where DetailID==@backSideID";
        //            else
        //                sqlcondition += "&& DetailID==@backSideID";
        //            SQLiteParameter param = new SQLiteParameter("@backSideID", backSide.DetailID);
        //            sqlCommand.Parameters.Add(param);
        //        }
        //        if (backSide.LessonID > -1)
        //        {
        //            if (string.IsNullOrWhiteSpace(sqlcondition))
        //                sqlcondition += "where LessonID==@lessonID";
        //            else
        //                sqlcondition += "&& LessonID==@lessonID";
        //            SQLiteParameter param = new SQLiteParameter("@lessonID", backSide.LessonID);
        //            sqlCommand.Parameters.Add(param);
        //        }
        //        sqlCommand.CommandText = sql + sqlcondition;
        //        reader = sqlCommand.ExecuteReader();

        //        while (reader.Read())
        //        {
        //            DepartmentDetailModel backSideModel = GetDepartmentDetailModel(reader);
        //            list.Add(backSideModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CatchException(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        sqlConnect.Dispose();
        //        sqlCommand.Dispose();
        //        reader.Dispose();
        //    }
        //    return list;
        //}




        //public IList<DepartmentDetailModel> GetAllWithRelation()
        //{
        //    List<DepartmentDetailModel> list = new List<DepartmentDetailModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    LessonDataAccess lessonDA = new LessonDataAccess();
        //    CategoryDataAccess categoryDA = new CategoryDataAccess();
        //    TypeDataAccess typeDA = new TypeDataAccess();
        //    string sql = "select * from departmentDetailModel ";
        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        sqlCommand.CommandText = sql;
        //        reader = sqlCommand.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            //DepartmentDetailModel
        //            DepartmentDetailModel backSideModel = GetDepartmentDetailModel(reader);
        //            //LessonModel
        //            backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
        //            //CategoryModel
        //            backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            //TypeModel
        //            backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            list.Add(backSideModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CatchException(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        sqlConnect.Dispose();
        //        sqlCommand.Dispose();
        //        reader.Dispose();
        //    }
        //    return list;
        //}

        //public IList<DepartmentDetailModel> GetAllWithRelation(int backSideID)
        //{
        //    List<DepartmentDetailModel> list = new List<DepartmentDetailModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    SQLiteParameter param = null;
        //    LessonDataAccess lessonDA = new LessonDataAccess();
        //    CategoryDataAccess categoryDA = new CategoryDataAccess();
        //    TypeDataAccess typeDA = new TypeDataAccess();
        //    string sql = "select * from departmentDetailModel where DetailID==@backSideID";
        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        sqlCommand.CommandText = sql;
        //        param = new SQLiteParameter("@backSideID", backSideID);
        //        sqlCommand.Parameters.Add(param);
        //        reader = sqlCommand.ExecuteReader();
        //        DepartmentDetailModel backSideModel;
        //        while (reader.Read())
        //        {
        //            //DepartmentDetailModel
        //            backSideModel = GetDepartmentDetailModel(reader);
        //            //LessonModel
        //            backSideModel.LessonModel = lessonDA.Get(backSideModel.LessonID);
        //            //CategoryModel
        //            backSideModel.LessonModel.CategoryModel = categoryDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            //TypeModel
        //            backSideModel.LessonModel.TypeModel = typeDA.Get(backSideModel.LessonModel.CategoryModel.CategoryID);
        //            list.Add(backSideModel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CatchException(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        sqlConnect.Dispose();
        //        sqlCommand.Dispose();
        //        reader.Dispose();
        //    }
        //    return list;
        //}


        public bool Insert(DepartmentDetailModel departmentDetailModel)
        {
            bool result = false;
            string sql = "insert into DepartmentDetails (DepartmentID,EmployeeID,LastActive,Status,FromDate,ToDate) values (@DepartmentID,@EmployeeID,@LastActive,@Status,@FromDate,@ToDate)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@DetailID", departmentDetailModel.DepartmentDetailID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@DepartmentID", departmentDetailModel.DepartmentID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@EmployeeID", departmentDetailModel.EmployeeID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@LastActive", departmentDetailModel.LastActive));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Status", departmentDetailModel.Status));
                sqlCommand.Parameters.Add(new SQLiteParameter("@FromDate", departmentDetailModel.FromDate));
                sqlCommand.Parameters.Add(new SQLiteParameter("@ToDate", departmentDetailModel.ToDate));
                sqlCommand.ExecuteNonQuery();
                departmentDetailModel.DepartmentDetailID = (int)sqlConnect.LastInsertRowId;
                departmentDetailModel.IsNew = false;
                departmentDetailModel.IsDelete = false;
                departmentDetailModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                CatchException(ex);
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }


        public bool Update(DepartmentDetailModel departmentDetailModel)
        {
            bool result = false;
            string sql = "Update departmentDetailModel set DepartmentID=@DepartmentID,EmployeeID=@EmployeeID,LastActive=@LastActive,Status=@Status,FromDate=@FromDate,ToDate=@ToDate where DetailID = @DetailID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@DetailID", departmentDetailModel.DepartmentDetailID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@DepartmentID", departmentDetailModel.DepartmentID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@EmployeeID", departmentDetailModel.EmployeeID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@LastActive", departmentDetailModel.LastActive));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Status", departmentDetailModel.Status));
                sqlCommand.Parameters.Add(new SQLiteParameter("@FromDate", departmentDetailModel.FromDate));
                sqlCommand.Parameters.Add(new SQLiteParameter("@ToDate", departmentDetailModel.ToDate));

                sqlCommand.ExecuteNonQuery();
                departmentDetailModel.IsNew = false;
                departmentDetailModel.IsDelete = false;
                departmentDetailModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                CatchException(ex);
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }

        public bool Delete(DepartmentDetailModel departmentDetailModel)
        {
            bool result = false;
            string sql = "Delete from DepartmentDetails where DetailID = @DetailID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@DetailID", departmentDetailModel.DepartmentDetailID));
                sqlCommand.ExecuteNonQuery();
                departmentDetailModel.IsNew = false;
                departmentDetailModel.IsDelete = false;
                departmentDetailModel.IsEdit = false;
            }
            catch (Exception ex)
            {
                CatchException(ex);
                sqlConnect.Dispose();
                sqlCommand.Dispose();
                throw;
            }

            return result;
        }


        private DepartmentDetailModel GetDepartmentDetailModel(SQLiteDataReader reader)
        {
            DepartmentDetailModel departmentDetailModel = new DepartmentDetailModel();
            departmentDetailModel.DepartmentDetailID = reader["DetailID"] == null ? 0 : int.Parse(reader["DetailID"].ToString());
            departmentDetailModel.DepartmentID = reader["DepartmentID"] == null ? 0 : int.Parse(reader["DepartmentID"].ToString());
            departmentDetailModel.EmployeeID = reader["EmployeeID"] == null ? 0 : int.Parse(reader["EmployeeID"].ToString());
            departmentDetailModel.LastActive = reader["LastActive"] == null ? DateTime.MinValue : DateTime.Parse(reader["LastActive"].ToString());
            if (reader["FromDate"] != null && !string.IsNullOrWhiteSpace(reader["FromDate"].ToString()))
                departmentDetailModel.FromDate = DateTime.Parse(reader["FromDate"].ToString());
            else
                departmentDetailModel.FromDate = null;


            if (reader["ToDate"] != null && !string.IsNullOrWhiteSpace(reader["ToDate"].ToString()))
                departmentDetailModel.ToDate = DateTime.Parse(reader["ToDate"].ToString());
            else
                departmentDetailModel.ToDate = null;
            

            departmentDetailModel.Status = reader["Status"] == null ? 0 : int.Parse(reader["Status"].ToString());

            departmentDetailModel.IsEdit = false;
            departmentDetailModel.IsNew = false;
            departmentDetailModel.IsDelete = false;
            return departmentDetailModel;
        }
        #endregion
    }
}
