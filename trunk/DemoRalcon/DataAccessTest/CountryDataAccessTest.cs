using DemoFalcon.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DemoFalcon.Model;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DataAccessTest
{
    
    
    /// <summary>
    ///This is a test class for CountryDataAccessTest and is intended
    ///to contain all CountryDataAccessTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CountryDataAccessTest
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
        ///A test for CountryDataAccess Constructor
        ///</summary>
        [TestMethod()]
        public void CountryDataAccessConstructorTest()
        {
            CountryDataAccess target = new CountryDataAccess();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Delete
        ///</summary>
        [TestMethod()]
        public void DeleteTest()
        {
            CountryDataAccess target = new CountryDataAccess(); // TODO: Initialize to an appropriate value
            CountryModel employeeModel = null; // TODO: Initialize to an appropriate value
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
            CountryDataAccess target = new CountryDataAccess(); // TODO: Initialize to an appropriate value
            int countryID = 0; // TODO: Initialize to an appropriate value
            CountryModel expected = null; // TODO: Initialize to an appropriate value
            CountryModel actual;
            actual = target.Get(countryID);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void GetAllTest()
        {
            CountryDataAccess target = new CountryDataAccess(); // TODO: Initialize to an appropriate value
            IList<CountryModel> expected = null; // TODO: Initialize to an appropriate value
            IList<CountryModel> actual;
            actual = target.GetAll();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetCountryModel
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DemoRalcon.exe")]
        public void GetCountryModelTest()
        {
            CountryDataAccess_Accessor target = new CountryDataAccess_Accessor(); // TODO: Initialize to an appropriate value
            SQLiteDataReader reader = null; // TODO: Initialize to an appropriate value
            CountryModel expected = null; // TODO: Initialize to an appropriate value
            CountryModel actual;
            actual = target.GetCountryModel(reader);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Insert
        ///</summary>
        [TestMethod()]
        public void InsertTest()
        {
            CountryDataAccess target = new CountryDataAccess(); // TODO: Initialize to an appropriate value
            CountryModel countryModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Insert(countryModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Update
        ///</summary>
        [TestMethod()]
        public void UpdateTest()
        {
            CountryDataAccess target = new CountryDataAccess(); // TODO: Initialize to an appropriate value
            CountryModel employeeModel = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Update(employeeModel);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
