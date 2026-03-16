using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;

namespace SeedPlan.Shared.Interfaces
{
    public interface IUserInventoryService
    {
        Task<List<Seed>> GetMySeeds();
        Task AddSeed(Seed seed);
        Task UpdateSeed(Seed seed);
        Task DeleteSeed(int id);
        Task<List<Seed>> GetSeedsReadyForSowing(); // Använder frostdatum från profilen
        Task<List<PlantSowingView>> GetCurrentSowingCalendar();

        Task<IEnumerable<IGrouping<string, Seed>>> GetMySeedsGrouped();
    }
}
