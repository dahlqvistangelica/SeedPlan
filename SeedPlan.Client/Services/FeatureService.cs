using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly Supabase.Client _supabase;

        public FeatureService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }
        public async Task<AppFeature?> GetLatestActiveFeatureAsync()
        {
            var response = await _supabase.From<AppFeature>()
                .Where(f => f.IsActive == true)
                .Order(f => f.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            return response.Models.FirstOrDefault();
        }

        public async Task<bool> HasUserSeenFeatureAsync(int featureId)
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if(string.IsNullOrEmpty(userId))
            {
                return true;
            }
            var response = await _supabase.From<UserSeenFeature>()
                .Where(x => x.UserId == userId && x.FeatureId == featureId)
                .Get();

            return response.Models.Any();
        }

        public async Task MarkFeatureAsSeenAsync(int featureId)
        {
            var userId = _supabase.Auth.CurrentUser?.Id;

            if(string.IsNullOrEmpty(userId))
            { return; }
            var seenRecord = new UserSeenFeature
            {
                UserId = userId,
                FeatureId = featureId
            };

            await _supabase.From<UserSeenFeature>().Insert(seenRecord);
        }
    }
}
