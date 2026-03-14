using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfile?> GetUserProfile();
        Task UpdateUserProfile(UserProfile profile);
        Task<DateTime?> GetUserLastFrostDate();
    }
}
