using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Interfaces
{
    public interface IUserSowingService
    {
        Task<List<Sowing>> GetMySowings();
        Task AddSowing(Sowing sowing);
        Task UpdateSowingStatus(int id, int status);
        Task DeleteSowing(int id);
    }
}
