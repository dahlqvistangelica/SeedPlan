using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Helpers
{
    public static class Mapper
    {
        // ── Garden ───────────────────────────────────────────────────────────

        public static Garden ToGarden(GardenDb db) => new Garden
        {
            Id   = db.Id,
            Name = db.Name,
            Year = db.Year,
            // Areas populated separately
        };

        public static GardenDb ToGardenDb(Garden garden, string userId) => new GardenDb
        {
            Id     = garden.Id,
            UserId = userId,
            Name   = garden.Name,
            Year   = garden.Year,
        };

        // ── CultivationArea ──────────────────────────────────────────────────

        public static CultivationArea ToCultivationArea(GardenPlan db) => new CultivationArea
        {
            Id        = db.Id,
            GardenId  = db.GardenId ?? 0,
            Name      = db.Name,
            WidthCm   = db.WidthCm,
            LengthCm  = db.LengthCm,
            Color     = db.Color,
            SortOrder = db.SortOrder,
            // Crops populated separately
        };

        public static GardenPlan ToGardenPlan(CultivationArea area, string userId) => new GardenPlan
        {
            Id        = area.Id,
            GardenId  = area.GardenId,
            UserId    = userId,
            Name      = area.Name,
            WidthCm   = area.WidthCm,
            LengthCm  = area.LengthCm,
            Color     = area.Color,
            SortOrder = area.SortOrder,
        };

        // ── PlantedCrop ──────────────────────────────────────────────────────

        public static PlantedCrop ToPlantedCrop(GardenPlanCrop db, Plant? plant) => new PlantedCrop
        {
            Id      = db.Id.ToString(),
            Plant   = plant ?? new Plant { Id = (int)db.PlantId, PlantName = "?" },
            CenterX = db.CenterX,
            CenterY = db.CenterY,
        };

        public static GardenPlanCrop ToGardenPlanCrop(PlantedCrop crop, long areaId) => new GardenPlanCrop
        {
            AreaId  = areaId,
            PlantId = crop.Plant.Id,
            CenterX = crop.CenterX,
            CenterY = crop.CenterY,
        };
    }
}
