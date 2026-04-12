using SeedPlan.Shared.Helpers;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using Supabase;
using System.Collections.Generic;
using System.Threading.Tasks;

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


    public async Task<List<CultivationArea>> GetUserPlansAsync(string userId)
    {
        var dbPlans = (await _supabase.From<GardenPlan>().Where(gp => gp.UserId == userId).Get()).Models;
        var plantLib = await _plantLibrary.GetAllPlantsAsync();

        var result = new List<CultivationArea>();
        foreach (var dbPlan in dbPlans)
        {
            var area = Mapper.ToCultivationArea(dbPlan);

            // Hämta crops för denna plan
            var dbCrops = (await _supabase.From<GardenPlanCrop>().Where(c => c.AreaId == dbPlan.Id).Get()).Models;
            area.Crops = dbCrops
                .Select(crop => Mapper.ToPlantedCrop(crop, plantLib.FirstOrDefault(p => p.Id == crop.PlantId)))
                .ToList();

            result.Add(area);
        }
        return result;
    }

    // Spara (insert eller update) en plan och dess crops
    public async Task SavePlanAsync(CultivationArea area, string userId)
    {
        // 1. Spara/uppdatera plan (insert om ny => nytt id tillbaka, annars update)
        var isNew = area is { Name: { Length: > 0 }, Id: 0 };
        GardenPlan dbPlan = Mapper.ToGardenPlan(area);
        dbPlan.UserId = userId;

        if (isNew)
        {
            dbPlan = (await _supabase.From<GardenPlan>().Insert(dbPlan)).Models.First();
            area.Id = dbPlan.Id;
        }
        else
        {
            await _supabase.From<GardenPlan>().Update(dbPlan);
        }

        // 2. Radera gamla crops (för att slippa dubbletter)
        await _supabase.From<GardenPlanCrop>().Where(c => c.AreaId == area.Id).Delete();

        // 3. Lägg in nya crops
        foreach (var crop in area.Crops)
        {
            var dbCrop = Mapper.ToGardenPlanCrop(crop, area.Id);
            await _supabase.From<GardenPlanCrop>().Insert(dbCrop);
        }
    }

    // Hämta EN plan med crops
    public async Task<CultivationArea?> GetPlanAsync(int areaId)
    {
        var dbPlan = (await _supabase.From<GardenPlan>().Where(gp => gp.Id == areaId).Get()).Models.FirstOrDefault();
        if (dbPlan == null) return null;

        var plantLib = await _plantLibrary.GetAllPlantsAsync();
        var dbCrops = (await _supabase.From<GardenPlanCrop>().Where(c => c.AreaId == areaId).Get()).Models;

        var area = Mapper.ToCultivationArea(dbPlan);
        area.Crops = dbCrops
            .Select(crop => Mapper.ToPlantedCrop(crop, plantLib.FirstOrDefault(p => p.Id == crop.PlantId)))
            .ToList();
        return area;
    }

    public async Task DeletePlanAsync(int areaId)
    {
        await _supabase.From<GardenPlan>().Where(gp => gp.Id == areaId).Delete();
        // crops raderas automatiskt pga on delete cascade
    }
}


}
