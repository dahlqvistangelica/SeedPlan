using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeedPlan.Shared.Helpers;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.UnitTests.Flows
{
    [TestClass]
    public class Prio3WorkflowTests
    {
        [TestMethod]
        public void StatusChain_0_To_7_IsFullyAllowed()
        {
            var path = new[]
            {
                (int)SowingStatus.Sown,
                (int)SowingStatus.Germinated,
                (int)SowingStatus.TrueLeaves,
                (int)SowingStatus.PottedOn,
                (int)SowingStatus.HardeningOff,
                (int)SowingStatus.PlantedOut,
                (int)SowingStatus.Harvested,
                (int)SowingStatus.Finished
            };

            for (var i = 0; i < path.Length - 1; i++)
            {
                Assert.IsTrue(
                    SowingStatusFlow.CanTransition(path[i], path[i + 1]),
                    $"Expected transition {path[i]} -> {path[i + 1]} to be valid.");
            }
        }

        [TestMethod]
        public void InvalidJump_3_To_5_IsBlocked()
        {
            Assert.IsFalse(
                SowingStatusFlow.CanTransition((int)SowingStatus.PottedOn, (int)SowingStatus.PlantedOut));
        }

        [TestMethod]
        public void ActiveStatuses_CanTransitionTo99()
        {
            var activeStatuses = new[] { 0, 1, 2, 3, 4, 5, 6 };

            foreach (var status in activeStatuses)
            {
                Assert.IsTrue(
                    SowingStatusFlow.CanTransition(status, (int)SowingStatus.Failed),
                    $"Expected {status} -> 99 to be valid for active status.");
            }

            Assert.IsFalse(SowingStatusFlow.CanTransition((int)SowingStatus.Finished, (int)SowingStatus.Failed));
            Assert.IsFalse(SowingStatusFlow.CanTransition((int)SowingStatus.Failed, (int)SowingStatus.Failed));
        }

        [TestMethod]
        public void DeletionRule_ReturnsSeedsOnlyForStatus0()
        {
            Assert.IsTrue(SowingDeletionRules.ShouldReturnSeedsToInventory((int)SowingStatus.Sown));

            Assert.IsFalse(SowingDeletionRules.ShouldReturnSeedsToInventory((int)SowingStatus.Germinated));
            Assert.IsFalse(SowingDeletionRules.ShouldReturnSeedsToInventory((int)SowingStatus.Harvested));
            Assert.IsFalse(SowingDeletionRules.ShouldReturnSeedsToInventory((int)SowingStatus.Finished));
            Assert.IsFalse(SowingDeletionRules.ShouldReturnSeedsToInventory((int)SowingStatus.Failed));
        }
    }
}