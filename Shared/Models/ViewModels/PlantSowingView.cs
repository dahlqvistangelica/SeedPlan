namespace SeedPlan.Shared.Models.ViewModels
{
    public class PlantSowingView
    {
        public Plant Plant { get; set; } = null!;
        public bool HasSeeds { get; set; }
        public List<Seed> OwnedSeeds { get; set; } = new();
    }
}
