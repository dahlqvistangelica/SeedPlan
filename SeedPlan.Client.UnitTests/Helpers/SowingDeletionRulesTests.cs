using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeedPlan.Shared.Helpers;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.UnitTests.Helpers
{
    [TestClass]
    public class SowingDeletionRulesTests
    {
        [TestMethod]
        public void ShouldReturnSeedsToInventory_Status0_ReturnsTrue()
        {
            Assert.IsTrue(SowingDeletionRules.ShouldReturnSeedsToInventory((int)SowingStatus.Sown));
        }

        [DataTestMethod]
        [DataRow((int)SowingStatus.Germinated)]
        [DataRow((int)SowingStatus.TrueLeaves)]
        [DataRow((int)SowingStatus.PottedOn)]
        [DataRow((int)SowingStatus.HardeningOff)]
        [DataRow((int)SowingStatus.PlantedOut)]
        [DataRow((int)SowingStatus.Harvested)]
        [DataRow((int)SowingStatus.Finished)]
        [DataRow((int)SowingStatus.Failed)]
        public void ShouldReturnSeedsToInventory_Status1OrHigher_ReturnsFalse(int status)
        {
            Assert.IsFalse(SowingDeletionRules.ShouldReturnSeedsToInventory(status));
        }
    }
}