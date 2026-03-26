namespace SeedPlan.Shared.Models
{
    public class UpdateSowingProgressRequest
    {
        public int SowingId { get; set; }
        public int? ActiveSeedlingCount { get; set; }
        public string? Notes { get; set; }
        public DateTime? EventDate { get; set; }
    }
}