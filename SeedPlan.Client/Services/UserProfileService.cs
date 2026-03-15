using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly Supabase.Client _supabase;

        // Här injiceras både Supabase och en annan tjänst automatiskt
        public UserProfileService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }
        //Hämta användarens profilinställningar
        public async Task<UserProfile?> GetUserProfile()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null)
            {
                return null;
            }
            var response = await _supabase
                .From<UserProfile>()
                .Where(x => x.Id == user.Id)
                .Single();
            return response;
        }
        public async Task UpdateUserProfile(UserProfile userProfile)
        {
            var user = _supabase.Auth.CurrentUser;
            if(user == null)
            {
                return;
            }

            userProfile.Id = user.Id;
            userProfile.UpdatedLast = DateTime.UtcNow;

            await _supabase
                .From<UserProfile>()
                .Where(x => x.Id == user.Id)
                .Update(userProfile);

        }
        public async Task<DateTime?> GetUserLastFrostDate()
        {
            var profile = await GetUserProfile();
            return profile?.LastFrostDate;
        }
    }
}

