using FlashCard.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Anito.Data;
using FlashCard.Model;
using System.Collections.Generic;

namespace TestDataAccessProject1
{
    
    
    /// <summary>
    ///This is a test class for LessonDataAccessTest and is intended
    ///to contain all LessonDataAccessTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LessonDataAccessTest
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
        ///A test for DataSession
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FlashCard.DataAccess.dll")]
        public void DataSessionTest()
        {
            LessonDataAccess_Accessor target = new LessonDataAccess_Accessor(); // TODO: Initialize to an appropriate value
            ISession actual;
            actual = target.DataSession;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void GetAllTest()
        {
            LessonDataAccess target = new LessonDataAccess(); // TODO: Initialize to an appropriate value
            IList<LessonModel> expected = null; // TODO: Initialize to an appropriate value
            IList<LessonModel> actual;
            actual = target.GetAll();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LessonDataAccess Constructor
        ///</summary>
        [TestMethod()]
        public void LessonDataAccessConstructorTest()
        {
            LessonDataAccess target = new LessonDataAccess();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
