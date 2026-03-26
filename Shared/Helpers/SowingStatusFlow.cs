using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Helpers
{
    public static class SowingStatusFlow
    {
        public static bool IsActive(int status) =>
            status >= (int)SowingStatus.Sown &&
            status <= (int)SowingStatus.Harvested;

        public static bool CanTransition(int currentStatus, int targetStatus)
        {
            // Failed can be reached from any active state, but not terminal states.
            if (targetStatus == (int)SowingStatus.Failed)
            {
                return IsActive(currentStatus);
            }

            if (currentStatus == targetStatus)
            {
                return IsActive(currentStatus);
            }

            return currentStatus >= (int)SowingStatus.Sown
                && currentStatus <= (int)SowingStatus.Harvested
                && targetStatus == currentStatus + 1;
        }
    }
}