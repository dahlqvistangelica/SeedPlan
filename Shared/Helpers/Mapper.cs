using SeedPlan.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Helpers
{
    public static class Mapper
    {
        public static CultivationArea ToCultivationArea(GardenPlan db)
            => new CultivationArea
            {
                Name = db.Name,
                WidthCm = db.WidthCm,
                LengthCm = db.LengthCm,
                // Crops sätts separat
            };

        public static GardenPlan ToGardenPlan(CultivationArea area)
            => new GardenPlan
            {
                Id = area.Id,
                Name = area.Name,
                WidthCm = area.WidthCm,
                LengthCm = area.LengthCm,
                // UserId sätts separat!
            };

        public static PlantedCrop ToPlantedCrop(GardenPlanCrop db, Plant? plant)
            => new PlantedCrop
            {
                Id = db.Id.ToString(),
                Plant = plant ?? new Plant { Id = db.PlantId, PlantName = "?" },
                CenterX = db.CenterX,
                CenterY = db.CenterY
            };

        public static GardenPlanCrop ToGardenPlanCrop(PlantedCrop crop, int areaId)
            => new GardenPlanCrop
            {
                AreaId = areaId,
                PlantId = crop.Plant.Id,
                CenterX = crop.CenterX,
                CenterY = crop.CenterY
            };
    }
}
