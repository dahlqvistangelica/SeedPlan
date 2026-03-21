using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;

namespace SeedPlan.Shared.Interfaces
{
    public interface IUserSowingService
    {
        Task<List<Sowing>> GetMySowings();
        Task<List<SowingView>> GetMySowingViews();
        Task AddSowing(Sowing sowing);
        Task UpdateSowingStatus(int id, int status);
        Task DeleteSowing(int id);
    }
}
