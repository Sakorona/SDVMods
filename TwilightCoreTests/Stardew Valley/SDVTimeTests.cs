using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwilightCore.StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilightCore.StardewValley.Tests
{
    [TestClass()]
    public class SDVTimeTests
    {
        [TestMethod()]
        public void TestIntTimeConv()
        {
            SDVTime Test = new SDVTime(1000);
            Assert.AreEqual(1000, Test.ReturnIntTime());
        }

        [TestMethod]
        public void TestTimeToString()
        {
            SDVTime Test = new SDVTime(1000);
            Assert.AreEqual("1000", Test.ToString());
        }

        [TestMethod]
        public void TestTimeToStringAfterMidnight()
        {
            SDVTime Test = new SDVTime(2512);
            Assert.AreEqual("0112", Test.ToString());
        }

        [TestMethod]
        public void TestTimeToStringPad()
        {
            SDVTime Test = new SDVTime(1005);
            Assert.AreEqual("1005", Test.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestInvalidTimeTooManyHours()
        {
            SDVTime Test = new SDVTime(2633);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestInvalidTimeStillAfterMidnight()
        {
            SDVTime Test = new SDVTime(2593);
        }

        [TestMethod]
        public void TestAddTime()
        {
            SDVTime Test = new SDVTime(2312) + new SDVTime(44);
            Assert.AreEqual(2356, Test.ReturnIntTime());
        }

        [TestMethod]
        public void TestAddTimeAroundHour()
        {
            SDVTime Test = new SDVTime(1256) + new SDVTime(156);
            Assert.AreEqual(1452, Test.ReturnIntTime());
        }

        [TestMethod]
        public void TestAddTimeAroundMidnight()
        {
            SDVTime Test = new SDVTime(2312) + new SDVTime(112);
            Assert.AreEqual("0024", Test.ToString());
        }

        [TestMethod]
        public void TestSubtractTimeAroundMidnight()
        {
            SDVTime Test = new SDVTime(2512) - new SDVTime(148);
            Assert.AreEqual(2324, Test.ReturnIntTime());
        }
    }
}