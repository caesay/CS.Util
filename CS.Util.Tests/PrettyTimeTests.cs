using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using CS.Util.Extensions;

namespace CS.Util.Tests
{
    [TestClass]
    public class PrettyTimeTests
    {
        [TestMethod]
        public void RightNow()
        {
            Assert.AreEqual("right now", DateTime.Now.AddMilliseconds(100).AsPrettyString());
        }

        [TestMethod]
        public void MinutesFromNow()
        {
            Assert.AreEqual("12 minutes from now", DateTime.Now.AddMinutes(12).AsPrettyString());
        }

        [TestMethod]
        public void HoursFromNow()
        {
            Assert.AreEqual("3 hours from now", DateTime.Now.AddHours(3).AsPrettyString());
        }

        [TestMethod]
        public void DaysFromNow()
        {
            Assert.AreEqual("3 days from now", DateTime.Now.AddDays(3).AsPrettyString());
        }

        [TestMethod]
        public void WeeksFromNow()
        {
            Assert.AreEqual("3 weeks from now", DateTime.Now.AddDays(21).AsPrettyString());
        }

        [TestMethod]
        public void MonthsFromNow()
        {
            Assert.AreEqual("3 months from now", DateTime.Now.AddMonths(3).AsPrettyString());
        }

        [TestMethod]
        public void YearsFromNow()
        {
            Assert.AreEqual("3 years from now", DateTime.Now.AddYears(3).AsPrettyString());
        }

        [TestMethod]
        public void DecadesFromNow()
        {
            Assert.AreEqual("30 years from now", DateTime.Now.AddYears(30).AsPrettyString());
        }

        [TestMethod]
        public void CenturiesFromNow()
        {
            Assert.AreEqual("300 years from now", DateTime.Now.AddYears(300).AsPrettyString());
        }

        [TestMethod]
        public void MomentsAgo()
        {
            Assert.AreEqual("moments ago", DateTime.Now.AddMilliseconds(-100).AsPrettyString());
        }

        [TestMethod]
        public void MinutesAgo()
        {
            Assert.AreEqual("12 minutes ago", DateTime.Now.AddMinutes(-12).AsPrettyString());
        }

        [TestMethod]
        public void HoursAgo()
        {
            Assert.AreEqual("3 hours ago", DateTime.Now.AddHours(-3).AsPrettyString());
        }

        [TestMethod]
        public void DaysAgo()
        {
            Assert.AreEqual("3 days ago", DateTime.Now.AddDays(-3).AsPrettyString());
        }

        [TestMethod]
        public void WeeksAgo()
        {
            Assert.AreEqual("3 weeks ago", DateTime.Now.AddDays(-21).AsPrettyString());
        }

        [TestMethod]
        public void MonthsAgo()
        {
            Assert.AreEqual("3 months ago", DateTime.Now.AddMonths(-3).AsPrettyString());
        }

        [TestMethod]
        public void YearsAgo()
        {
            Assert.AreEqual("3 years ago", DateTime.Now.AddYears(-3).AsPrettyString());
        }

        [TestMethod]
        public void DecadesAgo()
        {
            Assert.AreEqual("30 years ago", DateTime.Now.AddYears(-30).AsPrettyString());
        }

        [TestMethod]
        public void CenturiesAgo()
        {
            Assert.AreEqual("300 years ago", DateTime.Now.AddYears(-300).AsPrettyString());
        }
    }
}