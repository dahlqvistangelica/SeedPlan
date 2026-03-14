using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Interfaces
{
    public interface IPlantLibraryService
    {
        Task<List<Plant>> GetAllPlantsAsync();
        Task<List<Plant>> SearchPlantsAsync(string searchTerm);
        Task<List<Variety>> GetVarietiesForPlantAsync(int plantId);
        Task<List<Plant>> GetGeneralSowingSuggestionsAsync(DateTime lastFrostDate);

        Task<List<Variety>> GetAllVarietiesAsync();
    }
}
