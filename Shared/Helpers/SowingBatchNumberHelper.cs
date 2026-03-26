namespace SeedPlan.Shared.Helpers
{
    public static class SowingBatchNumberHelper
    {
        public static int GetNextBatchNumber(IEnumerable<int> existingBatchNumbers)
        {
            var next = existingBatchNumbers
                .Where(x => x > 0)
                .DefaultIfEmpty(0)
                .Max() + 1;

            return next;
        }
    }
}