using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeedPlan.Shared.Helpers;

namespace SeedPlan.Client.UnitTests.Helpers
{
    [TestClass]
    public class SowingBatchNumberHelperTests
    {
        [TestMethod]
        public void GetNextBatchNumber_WhenEmpty_ReturnsOne()
        {
            var result = SowingBatchNumberHelper.GetNextBatchNumber(Array.Empty<int>());
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetNextBatchNumber_WhenExistingValues_ReturnsMaxPlusOne()
        {
            var result = SowingBatchNumberHelper.GetNextBatchNumber(new[] { 1, 2, 4, 3 });
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void GetNextBatchNumber_IgnoresNonPositiveValues()
        {
            var result = SowingBatchNumberHelper.GetNextBatchNumber(new[] { 0, -2, 2 });
            Assert.AreEqual(3, result);
        }
    }
}