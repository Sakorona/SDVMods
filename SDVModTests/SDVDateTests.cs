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
    public class SDVDateTests
    {
        [TestMethod]
        public void CheckDateCreation()
        {
            SDVDate Test = new SDVDate("winter", 3);
            Assert.AreEqual("winter 3", Test.ToString());
        }

        [TestMethod]
        public void CheckDateCreationB()
        {
            SDVDate Test = new SDVDate("Winter", 3);
            Assert.AreEqual("winter 3", Test.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CheckInvalidDate()
        {
            SDVDate Test = new SDVDate("winter", 30);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CheckInvalidSeason()
        {
            SDVDate Test = new SDVDate("Thunder", 30);
        }

        [TestMethod]
        public void CheckEquals()
        {
            SDVDate Test = new SDVDate("summer", 4);
            SDVDate Test2 = new SDVDate("summer", 4);
            SDVDate Test3 = new SDVDate("summer", 9);
            SDVDate Test4 = new SDVDate("fall", 4);

            Assert.AreEqual(true, Test == Test2);
            Assert.AreEqual(false, Test == Test3);
            Assert.AreEqual(true, Test != Test4);
        }

        [TestMethod]
        public void CheckNextSeason()
        {
            Assert.AreEqual("fall", SDVDate.GetNextSeason("summer"));
            Assert.AreEqual("spring", SDVDate.GetNextSeason("winter"));
            Assert.AreEqual("Error", SDVDate.GetNextSeason("thunder"));
        }

        [TestMethod]
        public void CheckPrevSeason()
        {
            Assert.AreEqual("summer", SDVDate.GetPrevSeason("fall"));
            Assert.AreEqual("winter", SDVDate.GetPrevSeason("spring"));
        }

        [TestMethod]
        public void CheckDateMath()
        {
            SDVDate Test1 = new SDVDate("fall", 10);
            SDVDate Test2 = new SDVDate("spring", 27);
            SDVDate Test3 = new SDVDate("summer", 12);
            SDVDate Test4 = new SDVDate("summer", 20);
            SDVDate Test5 = new SDVDate("spring", 27);

            SDVDate Result1 = new SDVDate("fall", 12);
            SDVDate Result2 = new SDVDate("spring", 8);
            SDVDate Result3 = new SDVDate("summer", 8);
            SDVDate Result4 = new SDVDate("spring", 10);
            SDVDate Result5 = new SDVDate("summer", 2);

            //test +
            Assert.AreEqual(Result1, Test1 + 2);
            Assert.AreEqual(Result2, Test2 + 93);
            Assert.AreEqual(Result5, Test5 + 3);

            //test -
            Assert.AreEqual(Result3, Test3 - 4);
            Assert.AreEqual(Result4, Test4 - 38);
        }

        [TestMethod]
        public void CheckTommorowDate()
        {
            SDVDate Test = new SDVDate("fall", 28);
            int d = 28;
            string s = "spring";
            
            SDVDate Result1 = new SDVDate("winter", 1);
            SDVDate Result2 = new SDVDate("summer", 1);

            Assert.AreEqual(Result1, SDVDate.GetNextDay(Test));
            Assert.AreEqual(Result2, SDVDate.GetNextDay(s, d));
        }
    }
}