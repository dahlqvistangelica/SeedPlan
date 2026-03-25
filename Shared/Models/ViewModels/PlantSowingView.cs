namespace SeedPlan.Shared.Models.ViewModels
{
    public class PlantSowingView
    {
        public Plant Plant { get; set; } = null!;
        public bool HasSeeds { get; set; }
        public List<Seed> OwnedSeeds { get; set; } = new();

        public bool IsInNormalWindow { get; set; }
        public bool IsShifted { get; set; }      // "Delayed but possible"
        public bool HarvestAfterAug { get; set; }
        public DateTime? SowDate { get; set; }
        public DateTime? PlantOutDate { get; set; }
        public DateTime? HarvestDateEarly { get; set; }
        public DateTime? HarvestDateLate { get; set; }
    }
}
