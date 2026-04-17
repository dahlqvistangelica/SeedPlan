using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;

namespace SeedPlan.Shared.Interfaces
{
    public interface IPlantLibraryService
    {
        Task<List<Plant>> GetAllPlantsAsync();
        Task<List<PlantTag>> GetAllTagsAsync();
        Task<List<Plant>> SearchPlantsAsync(string searchTerm);
        Task<List<Variety>> GetVarietiesForPlantAsync(int plantId);
        Task<List<Plant>> GetGeneralSowingSuggestionsAsync(DateTime lastFrostDate);
        Task<List<PlantSowingView>> GetSowingCalendarAsync(DateTime lastFrostDate);
        Task<List<Variety>> GetAllVarietiesAsync();
        Task<Variety> AddVarietyAsync(Variety variety);
        Task<SowingOverview> GetSowingOverviewAsync(DateTime lastFrost);
        Task<Plant> UpdatePlantAsync(Plant plant);
        Task<Plant> AddPlantAsync(Plant plant);
    }
}
