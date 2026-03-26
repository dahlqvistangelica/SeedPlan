namespace SeedPlan.Shared.Models
{
    public enum SowingStatus
    {
        Sown = 0,
        Germinated = 1,
        TrueLeaves = 2,
        PottedOn = 3,
        HardeningOff = 4,
        PlantedOut = 5,
        Harvested = 6,
        Finished = 7,
        Failed = 99
    }

    public enum PlantCategory
    {
        Vegetable = 0,
        Flower = 1,
        Herb = 2,
        Fruit = 3
    }

}
