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
                .Where(x => x.UserId == userId)
                .Where(x => x.FeatureId == featureId)
                .Get();

            return response.Models.Any();
        }

        public async Task<string> GetLatestVersionAsync()
        {
            var version = await _supabase.From<AppFeature>()
                .Order(f => f.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            return version.Models.First().VersionTag;
        }

        public async Task MarkFeatureAsSeenAsync(int featureId)
        {
            try
            {
                var userId = _supabase.Auth.CurrentUser?.Id;

                if (string.IsNullOrEmpty(userId))
                { return; }
                var seenRecord = new UserSeenFeature
                {
                    UserId = userId,
                    FeatureId = featureId
                };

                await _supabase.From<UserSeenFeature>().Insert(seenRecord);
            }
            catch(Supabase.Postgrest.Exceptions.PostgrestException ex) when (ex.Message.Contains("23505") || ex.Message.Contains("duplicate"))
            {
                Console.WriteLine("Notis: funktionven var redan markerad som läst.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Kunde inte markera funktionen som läst." + ex.Message);
            }
        }
        
        public async Task<List<AppFeature>> GetAllFeaturesAsync()
        {
            var response = await _supabase.From<AppFeature>()
                .Order(f => f.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return response.Models;
        }

        
        public async Task AddFeatureAsync(AppFeature feature)
        {
            feature.CreatedAt = DateTime.UtcNow; 
            await _supabase.From<AppFeature>().Insert(feature);
        }

        
        public async Task UpdateFeatureAsync(AppFeature feature)
        {
            await _supabase.From<AppFeature>().Update(feature);
        }
    }
}
