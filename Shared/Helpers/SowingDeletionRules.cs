using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Helpers
{
    public static class SowingDeletionRules
    {
        public static bool ShouldReturnSeedsToInventory(int status)
        {
            return status < (int)SowingStatus.Germinated;
        }
    }
}