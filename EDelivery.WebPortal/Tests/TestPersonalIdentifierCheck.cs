using EDelivery.WebPortal.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestPersonalIdentifierCheck
    {
        public TestPersonalIdentifierCheck()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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


        [TestMethod]
        public void TestValidLnch()
        {
            var isValid = TextHelper.IsLNCh("1000157109");
            Assert.IsTrue(isValid);
            isValid = TextHelper.IsLNCh("1001392253");
            Assert.IsTrue(isValid);
            isValid = TextHelper.IsLNCh("1001280156");
            Assert.IsTrue(isValid);
            isValid = TextHelper.IsLNCh("1002159947");
            Assert.IsTrue(isValid);
            isValid = TextHelper.IsLNCh("1001169644");
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void TestInValidLnch()
        {
            var isValid = TextHelper.IsLNCh("1002159847");
            Assert.IsFalse(isValid);
            isValid = TextHelper.IsLNCh("1005159947 ");
            Assert.IsFalse(isValid);
           
        }
    }
}
