using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DemoFalcon.Model;
using System.Collections.ObjectModel;

namespace DemoFalcon.DataAccess
{
    public class DepartmentDataAccess : DataAccessBase
    {
        #region Contructors
        public DepartmentDataAccess()
        {
        }
        #endregion

        #region Methods
        public DepartmentModel Get(int departmentID)
        {
            DepartmentModel departmentModel = new DepartmentModel();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            string sql = "select * From Departments where DepartmentID ==@departmentID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@departmentID", departmentID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    departmentModel = GetDepartmentModel(reader);
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
            return departmentModel;
        }

        public IList<DepartmentModel> GetAll()
        {
            List<DepartmentModel> list = new List<DepartmentModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;

            SQLiteDataReader reader = null;
            string sql = "select * From Departments";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;

                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    DepartmentModel employeeModel = GetDepartmentModel(reader);
                    list.Add(employeeModel);
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

        //public IList<DepartmentModel> GetAll(DepartmentModel backSide)
        //{
        //    List<DepartmentModel> list = new List<DepartmentModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    string sql = "select * from Departments ";

        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        string sqlcondition = string.Empty;
        //        if (backSide.DepartmentID > -1)
        //        {
        //            if (string.IsNullOrWhiteSpace(sqlcondition))
        //                sqlcondition += "where DepartmentID==@backSideID";
        //            else
        //                sqlcondition += "&& DepartmentID==@backSideID";
        //            SQLiteParameter param = new SQLiteParameter("@backSideID", backSide.DepartmentID);
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
        //            DepartmentModel backSideModel = GetDepartmentModel(reader);
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




        public IList<DepartmentModel> GetAllWithRelation()
        {
            List<DepartmentModel> list = new List<DepartmentModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            DepartmentDetailDataAccess departmentDetailDA = new DepartmentDetailDataAccess();
            EmployeeDataAccess employeeDA = new EmployeeDataAccess();
            CountryDataAccess countryDA = new CountryDataAccess();
            string sql = "select * from Departments ";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    //DepartmentModel
                    DepartmentModel departmentModel = GetDepartmentModel(reader);
                    var detail = departmentDetailDA.GetAll().Where(x=>x.DepartmentID==departmentModel.DepartmentID);
                    departmentModel.DepartmentDetailCollection = new ObservableCollection<DepartmentDetailModel>(detail);
                    foreach (var item in departmentModel.DepartmentDetailCollection)
                    {
                        item.EmployeeModel = employeeDA.Get(item.EmployeeID);
                        item.EmployeeModel.CountryModel = countryDA.Get(item.EmployeeModel.CountryID);
                    }
                    departmentModel.IsEdit = false;
                    departmentModel.IsNew = false;
                  
                    list.Add(departmentModel);
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

        //public IList<DepartmentModel> GetAllWithRelation(int backSideID)
        //{
        //    List<DepartmentModel> list = new List<DepartmentModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    SQLiteParameter param = null;
        //    LessonDataAccess lessonDA = new LessonDataAccess();
        //    CategoryDataAccess categoryDA = new CategoryDataAccess();
        //    TypeDataAccess typeDA = new TypeDataAccess();
        //    string sql = "select * from Departments where DepartmentID==@backSideID";
        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        sqlCommand.CommandText = sql;
        //        param = new SQLiteParameter("@backSideID", backSideID);
        //        sqlCommand.Parameters.Add(param);
        //        reader = sqlCommand.ExecuteReader();
        //        DepartmentModel backSideModel;
        //        while (reader.Read())
        //        {
        //            //DepartmentModel
        //            backSideModel = GetDepartmentModel(reader);
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


        public bool Insert(DepartmentModel departmentModel)
        {
            bool result = false;
            string sql = "insert into Departments (DepartmentName) values (@DepartmentName)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@DepartmentID", departmentModel.DepartmentID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@DepartmentName", departmentModel.DepartmentName));
                sqlCommand.ExecuteNonQuery();
                departmentModel.DepartmentID = (int)sqlConnect.LastInsertRowId;
                departmentModel.IsNew = false;
                departmentModel.IsDelete = false;
                departmentModel.IsEdit = false;
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


        public bool Update(DepartmentModel employeeModel)
        {
            bool result = false;
            string sql = "Update Departments set DepartmentName=@DepartmentName where DepartmentID = @DepartmentID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@DepartmentID", employeeModel.DepartmentID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@DepartmentName", employeeModel.DepartmentName));

                sqlCommand.ExecuteNonQuery();
                employeeModel.IsNew = false;
                employeeModel.IsDelete = false;
                employeeModel.IsEdit = false;
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

        public bool Delete(DepartmentModel employeeModel)
        {
            bool result = false;
            string sql = "Delete from Departments where DepartmentID = @DepartmentID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@DepartmentID", employeeModel.DepartmentID));
                sqlCommand.ExecuteNonQuery();
                employeeModel.IsNew = false;
                employeeModel.IsDelete = false;
                employeeModel.IsEdit = false;
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


        private DepartmentModel GetDepartmentModel(SQLiteDataReader reader)
        {
            DepartmentModel employeeModel = new DepartmentModel();
            employeeModel.DepartmentID = reader["DepartmentID"] == null ? 0 : int.Parse(reader["DepartmentID"].ToString());
            employeeModel.DepartmentName = reader["DepartmentName"] == null ? string.Empty : reader["DepartmentName"].ToString();
            employeeModel.IsEdit = false;
            employeeModel.IsNew = false;
            employeeModel.IsDelete = false;
            return employeeModel;
        }
        #endregion
    }
}
