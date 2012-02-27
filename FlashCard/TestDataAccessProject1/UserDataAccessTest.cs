using FlashCard.DataAccess.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FlashCard.Model;
using System.Collections.Generic;

namespace TestDataAccessProject1
{
    
    
    /// <summary>
    ///This is a test class for UserDataAccessTest and is intended
    ///to contain all UserDataAccessTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UserDataAccessTest
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
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void GetAllTest()
        {
            UserDataAccess target = new UserDataAccess(); // TODO: Initialize to an appropriate value
            IList<UserModel> expected = null; // TODO: Initialize to an appropriate value
            IList<UserModel> actual;
            actual = target.GetAll();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UserDataAccess Constructor
        ///</summary>
        [TestMethod()]
        public void UserDataAccessConstructorTest()
        {
            UserDataAccess target = new UserDataAccess();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
