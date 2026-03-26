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
        Task<List<Seed>> GetSeedsReadyForSowing(); // Uses frost date from the profile
        Task<List<PlantSowingView>> GetCurrentSowingCalendar();

        Task<IEnumerable<IGrouping<string, SeedView>>> GetMySeedsGrouped();
    }
}
