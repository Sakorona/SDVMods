using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClimateOfFerngill;

namespace ClimateTesting
{
    [TestClass]
    public class FerngillMoonTesting
    {
        [TestMethod]
        public void VerifyPhase()
        {
            Assert.AreEqual(MoonPhase.NewMoon, SDVMoon.GetLunarPhase(16));
            Assert.AreEqual(MoonPhase.WaningGibbeous, SDVMoon.GetLunarPhase(74));
        }

        [TestMethod]
        public void VerifyLunarDescription()
        {
            Assert.AreEqual("Waxing Gibbeous", SDVMoon.DescribeMoonPhase(MoonPhase.WaxingGibbeous));
            Assert.AreEqual("Full Moon", SDVMoon.DescribeMoonPhase(MoonPhase.FullMoon));
        }
    }
}
