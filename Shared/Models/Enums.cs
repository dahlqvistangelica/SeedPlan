namespace SeedPlan.Shared.Models
{
    public enum SowingStatus
    {
        Sown = 0,
        Germinated = 1,
        TrueLeaves = 2,
        PottedOn1 = 3,
        PottedOn2 = 4,
        HardeningOff = 5,
        PlantedOut = 6,
        Harvested = 7,
        Failed = 99
    }
    public enum PlantCategory
    {
        Vegetable,
        Flower,
        Herb,
        Fruit
    }

}
