using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DemoFalcon.Model;
using System.Collections.ObjectModel;

namespace DemoFalcon.DataAccess
{
    public class EmployeeDataAccess : DataAccessBase
    {
        #region Contructors
        public EmployeeDataAccess()
        {

        }
        #endregion

        #region Properties


        #endregion

        #region Methods
        public EmployeeModel Get(int employeeID)
        {
            EmployeeModel employeeModel = new EmployeeModel();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            SQLiteParameter param = null;
            string sql = "select * From Employees where EmployeeID ==@employeeID";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                param = new SQLiteParameter("@employeeID", employeeID);
                sqlCommand.Parameters.Add(param);
                reader = sqlCommand.ExecuteReader();
                if (reader.Read())
                {
                    employeeModel = GetEmployeeModel(reader);
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
            return employeeModel;
        }

        public IList<EmployeeModel> GetAll()
        {
            List<EmployeeModel> list = new List<EmployeeModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;

            SQLiteDataReader reader = null;
            string sql = "select * From Employees";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;

                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    EmployeeModel employeeModel = GetEmployeeModel(reader);
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

        //public IList<EmployeeModel> GetAll(EmployeeModel backSide)
        //{
        //    List<EmployeeModel> list = new List<EmployeeModel>();
        //    SQLiteConnection sqlConnect = null;
        //    SQLiteCommand sqlCommand = null;
        //    SQLiteDataReader reader = null;
        //    string sql = "select * from Employees ";

        //    try
        //    {
        //        sqlConnect = new SQLiteConnection(ConnectionString);
        //        sqlConnect.Open();
        //        sqlCommand = new SQLiteCommand(sqlConnect);
        //        string sqlcondition = string.Empty;
        //        if (backSide.EmployeeID > -1)
        //        {
        //            if (string.IsNullOrWhiteSpace(sqlcondition))
        //                sqlcondition += "where EmployeeID==@backSideID";
        //            else
        //                sqlcondition += "&& EmployeeID==@backSideID";
        //            SQLiteParameter param = new SQLiteParameter("@backSideID", backSide.EmployeeID);
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
        //            EmployeeModel backSideModel = GetEmployeeModel(reader);
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



        /// <summary>
        /// OK
        /// </summary>
        /// <returns></returns>
        public IList<EmployeeModel> GetAllWithRelation()
        {
            List<EmployeeModel> list = new List<EmployeeModel>();
            SQLiteConnection sqlConnect = null;
            SQLiteCommand sqlCommand = null;
            SQLiteDataReader reader = null;
            DepartmentDetailDataAccess departmentDetailDA = new DepartmentDetailDataAccess();
            DepartmentDataAccess departmentDA = new DepartmentDataAccess();
            CountryDataAccess countryDA = new CountryDataAccess();
            string sql = "select * from Employees ";
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    //EmployeeModel
                    EmployeeModel employeeModel = GetEmployeeModel(reader);
                    var departmentDetail = departmentDetailDA.GetAll().Where(x => x.EmployeeID == employeeModel.EmployeeID);
                    employeeModel.DepartmentDetailCollection = new ObservableCollection<DepartmentDetailModel>(departmentDetail);
                    foreach (var item in employeeModel.DepartmentDetailCollection)
                    {
                        item.DepartmentModel = departmentDA.Get(item.DepartmentID);
                    }
                    //LessonModel
                    employeeModel.CountryModel = countryDA.Get(employeeModel.CountryID);
                    //CategoryModel
                    //employeeModel.DepartmentCollection = departmentDA.GetAl(employeeModel.EmployeeID);
                    
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

        public IList<EmployeeModel> GetAllWithRelation(int employeeID)
        {
            List<EmployeeModel> list = new List<EmployeeModel>();
            list = GetAllWithRelation().Where(x => x.EmployeeID == employeeID).ToList();
            return list;
        }


        public bool Insert(EmployeeModel employeeModel)
        {
            bool result = false;
            string sql = "insert into Employees (FirstName,MiddleName,LastName,Gender,BirthDate,Phone,Email,Address,Note,CountryID) values (@FirstName,@MiddleName,@LastName,@Gender,@BirthDate,@Phone,@Email,@Address,@Note,@CountryID)";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@EmployeeID", employeeModel.EmployeeID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@FirstName", employeeModel.FirstName));
                if (string.IsNullOrWhiteSpace(employeeModel.MiddleName))
                    employeeModel.MiddleName = string.Empty;
                sqlCommand.Parameters.Add(new SQLiteParameter("@MiddleName", employeeModel.MiddleName));
                sqlCommand.Parameters.Add(new SQLiteParameter("@LastName", employeeModel.LastName));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Gender", employeeModel.Gender));
                sqlCommand.Parameters.Add(new SQLiteParameter("@BirthDate", employeeModel.BirthDate));

                sqlCommand.Parameters.Add(new SQLiteParameter("@Phone", employeeModel.Phone));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Email", employeeModel.Email));

                sqlCommand.Parameters.Add(new SQLiteParameter("@Address", employeeModel.Address));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Note", employeeModel.Note));
                sqlCommand.Parameters.Add(new SQLiteParameter("@CountryID", employeeModel.CountryID));
                sqlCommand.ExecuteNonQuery();
                employeeModel.EmployeeID = (int)sqlConnect.LastInsertRowId;
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


        public bool Update(EmployeeModel employeeModel)
        {
            bool result = false;
            string sql = "Update Employees set FirstName=@FirstName,MiddleName=@MiddleName,LastName=@LastName,Gender=@Gender,BirthDate=@BirthDate,Phone=@Phone,Email=@Email,Address=@Address,Note=@Note,CountryID=@CountryID where EmployeeID = @EmployeeID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@EmployeeID", employeeModel.EmployeeID));
                sqlCommand.Parameters.Add(new SQLiteParameter("@FirstName", employeeModel.FirstName));
                sqlCommand.Parameters.Add(new SQLiteParameter("@MiddleName", employeeModel.MiddleName));
                sqlCommand.Parameters.Add(new SQLiteParameter("@LastName", employeeModel.LastName));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Gender", employeeModel.Gender));
                sqlCommand.Parameters.Add(new SQLiteParameter("@BirthDate", employeeModel.BirthDate));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Phone", employeeModel.Phone));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Email", employeeModel.Email));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Address", employeeModel.Address));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Note", employeeModel.Note));
                sqlCommand.Parameters.Add(new SQLiteParameter("@CountryID", employeeModel.CountryID));
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

        public bool Delete(EmployeeModel employeeModel)
        {
            bool result = false;
            string sql = "Delete from Employees where EmployeeID = @EmployeeID";
            SQLiteCommand sqlCommand = null;
            SQLiteConnection sqlConnect = null;
            try
            {
                sqlConnect = new SQLiteConnection(ConnectionString);
                sqlConnect.Open();
                sqlCommand = new SQLiteCommand(sqlConnect);
                sqlCommand.CommandText = sql;
                sqlCommand.Parameters.Add(new SQLiteParameter("@EmployeeID", employeeModel.EmployeeID));
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


        private EmployeeModel GetEmployeeModel(SQLiteDataReader reader)
        {
            EmployeeModel employeeModel = new EmployeeModel();
            employeeModel.EmployeeID = reader["EmployeeID"] == null ? 0 : int.Parse(reader["EmployeeID"].ToString());
            employeeModel.FirstName = reader["FirstName"].ToString();
            employeeModel.MiddleName = reader["MiddleName"].ToString();
            employeeModel.LastName = reader["LastName"].ToString();
            employeeModel.Gender = reader["Gender"] == null ? 0 : int.Parse(reader["Gender"].ToString());
            employeeModel.BirthDate = reader["BirthDate"] == null ? DateTime.MinValue : DateTime.Parse(reader["BirthDate"].ToString());
            employeeModel.Phone = reader["Phone"].ToString();
            employeeModel.Email = reader["Email"].ToString();
            employeeModel.Address = reader["Address"].ToString();
            employeeModel.Note = reader["Note"].ToString();
            employeeModel.CountryID = reader["CountryID"] == null ? 0 : int.Parse(reader["CountryID"].ToString());
            employeeModel.IsEdit = false;
            employeeModel.IsNew = false;
            employeeModel.IsDelete = false;
            return employeeModel;
        }
        #endregion
    }
}
