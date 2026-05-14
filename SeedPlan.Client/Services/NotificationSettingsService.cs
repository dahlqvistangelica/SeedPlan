using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly Supabase.Client _supabase;

        public NotificationSettingsService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<NotificationSettings> GetAsync()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new NotificationSettings();

            var response = await _supabase
                .From<NotificationSettings>()
                .Where(x => x.UserId == user.Id)
                .Get();

            return response.Models.FirstOrDefault() ?? new NotificationSettings
            {
                UserId = user.Id,
                Enabled = true,
                DaysBeforeSowing = [7, 2],
                DaysInactiveReminder = 14
            };
        }

        public async Task UpsertAsync(NotificationSettings settings)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;

            settings.UserId = user.Id;
            settings.UpdatedAt = DateTime.UtcNow;

            await _supabase
                .From<NotificationSettings>()
                .Upsert(settings);
        }
    }
}
