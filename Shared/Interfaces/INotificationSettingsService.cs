using SeedPlan.Shared.Models;

namespace SeedPlan.Shared.Interfaces
{
    public interface INotificationSettingsService
    {
        Task<NotificationSettings> GetAsync();
        Task UpsertAsync(NotificationSettings settings);
    }
}
