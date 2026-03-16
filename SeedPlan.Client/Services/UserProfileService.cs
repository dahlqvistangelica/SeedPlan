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
            var session = _supabase.Auth.CurrentSession;
            if (session == null)
            {
                session = await _supabase.Auth.RetrieveSessionAsync();
            }
            if(session?.User == null)
            {
                Console.WriteLine("DEBUG: Ingen användare hittades ens efter RetriveSession");
                return null;
            }
            try
            {
                var response = await _supabase
                    .From<UserProfile>()
                    .Where(x => x.Id == session.User.Id)
                    .Get();
                return response.Model;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"DEBUG: Fel vid hämtning av profil: {ex.Message}");
                return null;
            }
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

        public async Task UpsertUserProfile(UserProfile userProfile)
        {
            // Vi behöver inte filtrera med .Where() vid en Upsert om Id är PrimaryKey, 
            // Supabase sköter det automatiskt baserat på modellen.
            userProfile.UpdatedLast = DateTime.UtcNow;

            await _supabase
                .From<UserProfile>()
                .Upsert(userProfile);
        }
        public async Task<DateTime?> GetUserLastFrostDate()
        {
            var profile = await GetUserProfile();
            return profile?.LastFrostDate;
        }
    }
}

