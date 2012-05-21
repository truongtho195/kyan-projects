using DemoFalcon.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DemoFalcon.Model;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DataAccessTest
{
    
    
    /// <summary>
    ///This is a test class for DepartmentDetailDataAccessTest and is intended
    ///to contain all DepartmentDetailDataAccessTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DepartmentDetailDataAccessTest
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
        ///A test for DepartmentDetailDataAccess Constructor
        ///</summary>
        [TestMethod()]
        public void DepartmentDetailDataAccessConstructorTest()
        {
            DepartmentDetailDataAccess target = new DepartmentDetailDataAccess();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Delete
        ///</summary>
        [TestMethod()]
        public void DeleteTest()
        {
            DepartmentDetailDataAccess target = new DepartmentDetailDataAccess(); // TODO: Initialize to an appropriate value
            DepartmentDetailModel departmentDetailModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Delete(departmentDetailModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Get
        ///</summary>
        [TestMethod()]
        public void GetTest()
        {
            DepartmentDetailDataAccess target = new DepartmentDetailDataAccess(); // TODO: Initialize to an appropriate value
            int departmentDetailID = 0; // TODO: Initialize to an appropriate value
            DepartmentDetailModel expected = null; // TODO: Initialize to an appropriate value
            DepartmentDetailModel actual;
            actual = target.Get(departmentDetailID);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void GetAllTest()
        {
            DepartmentDetailDataAccess target = new DepartmentDetailDataAccess(); // TODO: Initialize to an appropriate value
            IList<DepartmentDetailModel> expected = null; // TODO: Initialize to an appropriate value
            IList<DepartmentDetailModel> actual;
            actual = target.GetAll();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetDepartmentDetailModel
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DemoRalcon.exe")]
        public void GetDepartmentDetailModelTest()
        {
            DepartmentDetailDataAccess_Accessor target = new DepartmentDetailDataAccess_Accessor(); // TODO: Initialize to an appropriate value
            SQLiteDataReader reader = null; // TODO: Initialize to an appropriate value
            DepartmentDetailModel expected = null; // TODO: Initialize to an appropriate value
            DepartmentDetailModel actual;
            actual = target.GetDepartmentDetailModel(reader);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Insert
        ///</summary>
        [TestMethod()]
        public void InsertTest()
        {
            DepartmentDetailDataAccess target = new DepartmentDetailDataAccess(); // TODO: Initialize to an appropriate value
            DepartmentDetailModel departmentDetailModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Insert(departmentDetailModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateTest()
        {
            DepartmentDetailDataAccess target = new DepartmentDetailDataAccess(); // TODO: Initialize to an appropriate value
            DepartmentDetailModel departmentDetailModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Update(departmentDetailModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
