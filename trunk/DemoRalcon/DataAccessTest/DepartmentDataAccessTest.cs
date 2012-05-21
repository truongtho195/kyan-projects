using DemoFalcon.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DemoFalcon.Model;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DataAccessTest
{
    
    
    /// <summary>
    ///This is a test class for DepartmentDataAccessTest and is intended
    ///to contain all DepartmentDataAccessTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DepartmentDataAccessTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for DepartmentDataAccess Constructor
        ///</summary>
        [TestMethod()]
        public void DepartmentDataAccessConstructorTest()
        {
            DepartmentDataAccess target = new DepartmentDataAccess();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Delete
        ///</summary>
        [TestMethod()]
        public void DeleteTest()
        {
            DepartmentDataAccess target = new DepartmentDataAccess(); // TODO: Initialize to an appropriate value
            DepartmentModel employeeModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Delete(employeeModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod()]
        public void GetTest()
        {
            DepartmentDataAccess target = new DepartmentDataAccess(); // TODO: Initialize to an appropriate value
            int departmentID = 0; // TODO: Initialize to an appropriate value
            DepartmentModel expected = null; // TODO: Initialize to an appropriate value
            DepartmentModel actual;
            actual = target.Get(departmentID);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void GetAllTest()
        {
            DepartmentDataAccess target = new DepartmentDataAccess(); // TODO: Initialize to an appropriate value
            IList<DepartmentModel> expected = null; // TODO: Initialize to an appropriate value
            IList<DepartmentModel> actual;
            actual = target.GetAll();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetDepartmentModel
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DemoRalcon.exe")]
        public void GetDepartmentModelTest()
        {
            DepartmentDataAccess_Accessor target = new DepartmentDataAccess_Accessor(); // TODO: Initialize to an appropriate value
            SQLiteDataReader reader = null; // TODO: Initialize to an appropriate value
            DepartmentModel expected = null; // TODO: Initialize to an appropriate value
            DepartmentModel actual;
            actual = target.GetDepartmentModel(reader);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Insert
        ///</summary>
        [TestMethod()]
        public void InsertTest()
        {
            DepartmentDataAccess target = new DepartmentDataAccess(); // TODO: Initialize to an appropriate value
            DepartmentModel departmentModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Insert(departmentModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateTest()
        {
            DepartmentDataAccess target = new DepartmentDataAccess(); // TODO: Initialize to an appropriate value
            DepartmentModel employeeModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Update(employeeModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
