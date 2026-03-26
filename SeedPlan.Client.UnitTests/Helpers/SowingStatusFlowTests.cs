using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeedPlan.Shared.Helpers;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.UnitTests.Helpers
{
    [TestClass]
    public class SowingStatusFlowTests
    {
        [TestMethod]
        public void CanTransition_ValidLinearPath_0_To_7_IsAllowed()
        {
            Assert.IsTrue(SowingStatusFlow.CanTransition((int)SowingStatus.Sown, (int)SowingStatus.Germinated));
            Assert.IsTrue(SowingStatusFlow.CanTransition((int)SowingStatus.Germinated, (int)SowingStatus.TrueLeaves));
            Assert.IsTrue(SowingStatusFlow.CanTransition((int)SowingStatus.TrueLeaves, (int)SowingStatus.PottedOn));
            Assert.IsTrue(SowingStatusFlow.CanTransition((int)SowingStatus.PottedOn, (int)SowingStatus.HardeningOff));
            Assert.IsTrue(SowingStatusFlow.CanTransition((int)SowingStatus.HardeningOff, (int)SowingStatus.PlantedOut));
            Assert.IsTrue(SowingStatusFlow.CanTransition((int)SowingStatus.PlantedOut, (int)SowingStatus.Harvested));
            Assert.IsTrue(SowingStatusFlow.CanTransition((int)SowingStatus.Harvested, (int)SowingStatus.Finished));
        }

        [TestMethod]
        public void CanTransition_InvalidJump_3_To_5_IsBlocked()
        {
            Assert.IsFalse(SowingStatusFlow.CanTransition((int)SowingStatus.PottedOn, (int)SowingStatus.PlantedOut));
        }

        [DataTestMethod]
        [DataRow((int)SowingStatus.Sown, true)]
        [DataRow((int)SowingStatus.Germinated, true)]
        [DataRow((int)SowingStatus.TrueLeaves, true)]
        [DataRow((int)SowingStatus.PottedOn, true)]
        [DataRow((int)SowingStatus.HardeningOff, true)]
        [DataRow((int)SowingStatus.PlantedOut, true)]
        [DataRow((int)SowingStatus.Harvested, true)]
        [DataRow((int)SowingStatus.Finished, false)]
        [DataRow((int)SowingStatus.Failed, false)]
        public void CanTransition_X_To_99_Allowed_FromActiveOnly(int fromStatus, bool expected)
        {
            var allowed = SowingStatusFlow.CanTransition(fromStatus, (int)SowingStatus.Failed);
            Assert.AreEqual(expected, allowed);
        }
    }
}