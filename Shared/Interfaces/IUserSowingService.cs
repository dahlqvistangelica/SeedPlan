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
        Task UpdateSowingStatusAsync(UpdateSowingStatusRequest request);
        Task UpdateSowingProgressAsync(UpdateSowingProgressRequest request);
        Task DeleteSowing(int id);
        Task<DeleteSowingResult> DeleteSowingWithResult(int id);
        Task<List<SowingEvent>> GetSowingEventsAsync(int sowingId);
        Task<int> GetNextBatchNumberAsync(int seedId);
    }
}
