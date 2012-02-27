using FlashCard.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestDataAccessProject1
{
    
    
    /// <summary>
    ///This is a test class for DataAccessBaseTest and is intended
    ///to contain all DataAccessBaseTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataAccessBaseTest
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
        ///A test for ConnectionString
        ///</summary>
        [TestMethod()]
        public void ConnectionStringTest()
        {
            string actual;
            actual = DataAccessBase.ConnectionString;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CatchException
        ///</summary>
        [TestMethod()]
        [DeploymentItem("FlashCard.DataAccess.dll")]
        public void CatchExceptionTest()
        {
            DataAccessBase_Accessor target = new DataAccessBase_Accessor(); // TODO: Initialize to an appropriate value
            Exception ex = null; // TODO: Initialize to an appropriate value
            target.CatchException(ex);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for DataAccessBase Constructor
        ///</summary>
        [TestMethod()]
        public void DataAccessBaseConstructorTest()
        {
            DataAccessBase target = new DataAccessBase();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
