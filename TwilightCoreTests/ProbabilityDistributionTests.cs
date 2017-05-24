using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TwilightCore.Tests
{
    [TestClass()]
    public class ProbabilityDistributionTests
    {
        [TestMethod()]
        public void TestAddingInScopeEndPoint()
        {
            double endPoint = .45;
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(endPoint, "Test");
            Assert.AreEqual(endPoint, Testing.GetCurrentEndPoint());
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestAddingOutOfScopeEndPoint()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(-.5, "Test");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestAddingOutOfScopeEndPoint2()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.95, "Test");
            Testing.AddNewEndPoint(.34, "Test 2");
        }
        
        [TestMethod]
        public void AddingTwoEndPoints()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.45, "Test");
            Testing.AddNewEndPoint(.35, "Test 2");
            Assert.AreEqual(.8, Testing.GetCurrentEndPoint());
        }

        [TestMethod]
        public void RetrievingOdds()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.55, "Test");
            Testing.GetEntryFromProb(.42, out string Odds);
            Assert.AreEqual("Test", Odds);
        }

        [TestMethod]
        public void CheckingOddsIncludeEnds()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.55, "Test");
            Testing.GetEntryFromProb(.55, out string Odds);
            Assert.AreEqual("Test", Odds);
        }

        [TestMethod]
        public void CheckingOddExcludeEnds()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.55, "Test");
            Testing.GetEntryFromProb(.55, out string Odds, false);
            Assert.AreEqual(null, Odds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CheckingParameterUnderAllowed()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.55, "Test");
            Testing.GetEntryFromProb(-3, out string Odds);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CheckingParameterOverAllowed()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.55, "Test");
            Testing.GetEntryFromProb(3, out string Odds);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNoProbAdded()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.GetEntryFromProb(.3, out string Odds);
        }

        [TestMethod]
        public void CheckOverflowWorking()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.55, "Test");
            Testing.SetOverflowResult("Hello");
            Testing.GetEntryFromProb(.6, out string Odds);
            Assert.AreEqual("Hello", Odds);
        }

        [TestMethod]
        public void CheckOverflowMissing()
        {
            var Testing = new ProbabilityDistribution<string>();
            Testing.AddNewEndPoint(.55, "Test");
            Testing.GetEntryFromProb(.6, out string Odds);
            Assert.AreEqual(null, Odds);
        }


    }
}