using SeedPlan.Shared.Helpers;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class GardenPlanService
    {
        private readonly Supabase.Client _supabase;
        private readonly IPlantLibraryService _plantLibrary;

        public GardenPlanService(Supabase.Client supabase, IPlantLibraryService plantLibrary)
        {
            _supabase = supabase;
            _plantLibrary = plantLibrary;
        }

        // ── Gardens ──────────────────────────────────────────────────────────

        public async Task<List<Garden>> GetUserGardensAsync(string userId)
        {
            var plantLib = await _plantLibrary.GetAllPlantsAsync();

            var dbGardens = (await _supabase.From<GardenDb>()
                .Where(g => g.UserId == userId)
                .Order("year", Supabase.Postgrest.Constants.Ordering.Descending)
                .Get()).Models;

            var dbAreas = (await _supabase.From<GardenPlan>()
                .Where(a => a.UserId == userId)
                .Order("sort_order", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get()).Models;

            var areaIds = dbAreas.Select(a => a.Id).ToList();
            List<GardenPlanCrop> allCrops = new();
            if (areaIds.Any())
            {
                allCrops = (await _supabase.From<GardenPlanCrop>()
                    .Filter("area_id", Supabase.Postgrest.Constants.Operator.In, areaIds)
                    .Get()).Models;
            }

            var result = new List<Garden>();
            foreach (var dbGarden in dbGardens)
            {
                var garden = Mapper.ToGarden(dbGarden);
                garden.Areas = dbAreas
                    .Where(a => a.GardenId == dbGarden.Id)
                    .Select(a =>
                    {
                        var area = Mapper.ToCultivationArea(a);
                        area.Crops = allCrops
                            .Where(c => c.AreaId == a.Id)
                            .Select(c => Mapper.ToPlantedCrop(c, plantLib.FirstOrDefault(p => p.Id == c.PlantId)))
                            .ToList();
                        return area;
                    })
                    .ToList();
                result.Add(garden);
            }
            return result;
        }

        public async Task<Garden> SaveGardenAsync(Garden garden, string userId)
        {
            var dbGarden = Mapper.ToGardenDb(garden, userId);
            if (garden.Id == 0)
            {
                dbGarden = (await _supabase.From<GardenDb>().Insert(dbGarden)).Models.First();
                garden.Id = dbGarden.Id;
            }
            else
            {
                await _supabase.From<GardenDb>().Update(dbGarden);
            }
            return garden;
        }

        public async Task DeleteGardenAsync(long gardenId)
        {
            // Areas and crops cascade-delete via FK
            await _supabase.From<GardenDb>().Where(g => g.Id == gardenId).Delete();
        }

        // ── Areas ─────────────────────────────────────────────────────────────

        public async Task<CultivationArea> SaveAreaAsync(CultivationArea area, string userId)
        {
            var dbArea = Mapper.ToGardenPlan(area, userId);
            if (area.Id == 0)
            {
                dbArea = (await _supabase.From<GardenPlan>().Insert(dbArea)).Models.First();
                area.Id = dbArea.Id;
            }
            else
            {
                await _supabase.From<GardenPlan>().Update(dbArea);
            }

            // Replace crops: delete old, insert new
            await _supabase.From<GardenPlanCrop>().Filter("area_id", Supabase.Postgrest.Constants.Operator.Equals, area.Id.ToString()).Delete();
            foreach (var crop in area.Crops)
            {
                await _supabase.From<GardenPlanCrop>().Insert(Mapper.ToGardenPlanCrop(crop, area.Id));
            }

            return area;
        }

        public async Task DeleteAreaAsync(long areaId)
        {
            // Crops cascade-delete
            await _supabase.From<GardenPlan>().Where(a => a.Id == areaId).Delete();
        }

        // ── Full garden save (garden + all areas) ─────────────────────────────

        public async Task SaveFullGardenAsync(Garden garden, string userId)
        {
            await SaveGardenAsync(garden, userId);
            for (int i = 0; i < garden.Areas.Count; i++)
            {
                garden.Areas[i].GardenId = garden.Id;
                garden.Areas[i].SortOrder = i;
                await SaveAreaAsync(garden.Areas[i], userId);
            }
        }
    }
}
