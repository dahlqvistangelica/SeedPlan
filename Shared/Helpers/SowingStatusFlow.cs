using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Helpers
{
    public static class SowingStatusFlow
    {
        private static readonly Dictionary<SowingStatus, HashSet<SowingStatus>> AllowedTransitions = new()
        {
            { SowingStatus.Sown, new HashSet<SowingStatus> { SowingStatus.Germinated, SowingStatus.TrueLeaves, SowingStatus.PottedOn, SowingStatus.Failed } },
            { SowingStatus.Germinated, new HashSet<SowingStatus> { SowingStatus.TrueLeaves, SowingStatus.PottedOn, SowingStatus.HardeningOff, SowingStatus.Failed } },
            { SowingStatus.TrueLeaves, new HashSet<SowingStatus> { SowingStatus.PottedOn, SowingStatus.HardeningOff, SowingStatus.Failed } },
            { SowingStatus.PottedOn, new HashSet<SowingStatus> { SowingStatus.HardeningOff, SowingStatus.PlantedOut, SowingStatus.Failed } },
            { SowingStatus.HardeningOff, new HashSet<SowingStatus> { SowingStatus.PlantedOut, SowingStatus.Failed } },
            { SowingStatus.PlantedOut, new HashSet<SowingStatus> { SowingStatus.Harvested, SowingStatus.Failed } },
            { SowingStatus.Harvested, new HashSet<SowingStatus> { SowingStatus.Finished, SowingStatus.Failed } },
            { SowingStatus.Finished, new HashSet<SowingStatus>() },
            { SowingStatus.Failed, new HashSet<SowingStatus>() }
        };

        public static bool IsActive(int status) =>
            status >= (int)SowingStatus.Sown &&
            status <= (int)SowingStatus.Harvested;

        public static bool CanTransition(int currentStatus, int targetStatus)
        {
            if (!Enum.IsDefined(typeof(SowingStatus), currentStatus) ||
                !Enum.IsDefined(typeof(SowingStatus), targetStatus))
            {
                return false;
            }

            // Always allow to mark as failed (99)
            if (targetStatus == (int)SowingStatus.Failed) return true;

            // If sowing already is finished or failed we don't allow any new events without recovery first.
            if (currentStatus == (int)SowingStatus.Finished || currentStatus == (int)SowingStatus.Failed) return false;

            // Allow to repeat/stay on pottedon multiple times
            if (currentStatus == (int)SowingStatus.PottedOn && targetStatus == (int)SowingStatus.PottedOn) return true;

            // Allow all transitions that moves forwad in the growing process (or directly to finished).
            return targetStatus > currentStatus || targetStatus == (int)SowingStatus.Finished;
        }
    }
}