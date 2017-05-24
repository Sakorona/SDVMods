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
            Assert.AreEqual(MoonPhase.ErrorPhase, SDVMoon.GetLunarPhase(-1));
        }

        [TestMethod]
        public void VerifyLunarDescription()
        {
            Assert.AreEqual("Waxing Gibbeous", MoonPhase.WaxingGibbeous);
            Assert.AreEqual("Full Moon", MoonPhase.FullMoon);
        }
    }
}
