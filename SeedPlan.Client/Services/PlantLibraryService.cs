using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using Shared.Models;

namespace SeedPlan.Client.Services
{
    public class PlantLibraryService: IPlantLibraryService
    {
        private readonly Supabase.Client _supabase;
        private readonly IUserProfileService _profileService;

        public PlantLibraryService(Supabase.Client supabase, IUserProfileService profileService)
        {
            _supabase = supabase;
            _profileService = profileService;
        }
        //Hämtar växtförslag beroende på vad användaren skriver.
        public async Task<List<Plant>> SearchPlantsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Plant>();
            }
            var response = await _supabase
                .From<Plant>()
                .Filter("plant_name", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Limit(10)
                .Get();
            return response.Models;

        }

        //Hämtar specifika sorter för en vald växt
        public async Task<List<Variety>> GetVarietiesForPlantAsync(int plantId)
        {
            var response = await _supabase
                .From<Variety>()
                .Where(v => v.PlantId == plantId)
                .Get();
            return response.Models;
        }

        public async Task<List<Variety>> GetAllVarietiesAsync()
        {
            var response = await _supabase
                .From<Variety>()
                .Get();
            return response.Models;
        }

        //Hämta alla växter i biblioteket
        public async Task<List<Plant>> GetAllPlantsAsync()
        {
            var response = await _supabase
                .From<Plant>()
                .Order(x => x.PlantName, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();
            return response.Models;

        }
        public async Task<List<Plant>> GetGeneralSowingSuggestionsAsync(DateTime lastFrostDate)
        {
            var allPlants = await GetAllPlantsAsync();
            var today = DateTime.Today;

            return allPlants
                .Select(plant => {
                    var outDate = lastFrostDate.AddDays(-(plant.WeeksBeforeFrost * 7));
                    var sowDate = outDate.AddDays(-(plant.SowingLeadTime * 7));
                    return new { Plant = plant, SowDate = sowDate };
                })
                .Where(x => Math.Abs((x.SowDate - today).TotalDays) <= 7)
                .OrderBy(x => x.SowDate) // Sortera så de mest aktuella kommer först
                .Select(x => x.Plant)
                .ToList();
        }

        public async Task<Variety> AddVarietyAsync(Variety variety)
        {
            var response = await _supabase
                .From<Variety>()
                .Insert(variety);
            return response.Model;
        }

        public async Task<SowingOverview> GetSowingOverviewAsync(DateTime lastFrost)
        {
            var allPlants = await GetAllPlantsAsync();
            var today = DateTime.Today;
            var overview = new SowingOverview();

            foreach(var plant in allPlants)
            {
                var start = lastFrost.AddDays(-(plant.SowingLeadTime * 7));

                var end = start.AddDays(28);
                if(today > end)
                {
                    overview.Past.Add(plant);
                }
                else if(today >= start && today <= end)
                {
                    overview.Current.Add(plant);
                }
                else
                {
                    overview.Upcoming.Add(plant);
                }


            }
            overview.Current = overview.Current.OrderBy(p => lastFrost.AddDays(-(p.SowingLeadTime * 7))).ToList();

            return overview;
        }
    }
}
