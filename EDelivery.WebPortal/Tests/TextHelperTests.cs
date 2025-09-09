using System;

using EDelivery.WebPortal.Utils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TextHelperTests
    {


        [TestMethod]
        public void TestIsNotEGNButLNCh()
        {
            string personalIdentifier = "1001589358";

            bool isEGN = TextHelper.IsEGN(personalIdentifier);
            bool isLNCh = TextHelper.IsLNCh(personalIdentifier);

            Assert.AreEqual(isEGN, false);
            Assert.AreEqual(isLNCh, true);
        }

        [TestMethod]
        public void TestGetBirthDateFromLNCh()
        {
            string LNCh = "1001589358";
            DateTime dt = TextHelper.GetBirthDateFromEGN(LNCh);

            Assert.AreEqual(dt.Year, TextHelper.DefaultDate.Year);
            Assert.AreEqual(dt.Month, TextHelper.DefaultDate.Month);
            Assert.AreEqual(dt.Day, TextHelper.DefaultDate.Day);
        }

        [TestMethod]
        public void TestGetBirthDateFromEGN()
        {
            string EGN = "0151081413";
            DateTime expected = new DateTime(1985, 7, 27);
            DateTime dt = TextHelper.GetBirthDateFromEGN(EGN);

            Assert.AreEqual(dt.Year, expected.Year);
            Assert.AreEqual(dt.Month, expected.Month);
            Assert.AreEqual(dt.Day, expected.Day);
        }
    }
}
