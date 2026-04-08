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

    public enum DahliaType
    {
        Decorative,
        Cactus,
        SemiCactus,
        Ball,
        Pompon,
        Anemone,
        Waterlily,
        Single,
        Collarette,
        Dinnerplate,
        Mignon,
        DecorativeDwarfs, 
        DecorativeGiants,
        DecorativeSmall,
        DecorativeLarge,
        DwarfCactus,
        Exclusive,
        Fringed,
        Orchid,
        Peony,
        Stellar,
        Other
    }

    public enum DahliaSize
    {
        Under5,
        Size5to10, 
        Size10to15,
        Size15to20,
        Size20to25,
        Over25
    }

}
