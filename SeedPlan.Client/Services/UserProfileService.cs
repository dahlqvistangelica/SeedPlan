using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly Supabase.Client _supabase;

        public UserProfileService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }
        /// <summary>
        /// Retrieves the current authenticated user's profile asynchronously.
        /// </summary>
        /// <remarks>This method returns null if there is no authenticated user or if the profile cannot
        /// be retrieved. Ensure that the user is authenticated before calling this method.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user's profile if the user
        /// is authenticated and a profile exists; otherwise, null.</returns>
        public async Task<UserProfile?> GetUserProfile()
        {
            
            try
            {
                await _supabase.InitializeAsync();

                var user = _supabase.Auth.CurrentUser;
                if (user == null) return null;

                var response = await _supabase
                    .From<UserProfile>()
                    .Where(x => x.Id == user.Id)
                    .Get();

                return response.Model;
            }
            catch(Exception ex)
            {
                                return null;
            }
        }
        /// <summary>
        /// Updates the current authenticated user's profile with the specified information.
        /// </summary>
        /// <remarks>If there is no authenticated user, the method completes without performing any
        /// update. The method updates the profile for the currently authenticated user only.</remarks>
        /// <param name="userProfile">The user profile data to update. The profile's identifier and last updated timestamp are set automatically
        /// based on the current authenticated user.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public async Task UpdateUserProfile(UserProfile userProfile)
        {
            await _supabase.InitializeAsync();

            var user = _supabase.Auth.CurrentUser;
            if(user == null)
            {
                return;
            }

            userProfile.Id = user.Id;
            userProfile.UpdatedLast = DateTime.UtcNow;

            var existingProfile = await _supabase
                .From<UserProfile>()
                .Where(x => x.Id == user.Id)
                .Get();

            if (existingProfile.Model == null)
            {
                await _supabase
                    .From<UserProfile>()
                    .Insert(userProfile);
                return;
            }

            await _supabase
                .From<UserProfile>()
                .Where(x => x.Id == user.Id)
                .Update(userProfile);

        }
        /// <summary>
        /// Inserts a new user profile or updates an existing one in the data store asynchronously.
        /// </summary>
        /// <remarks>If a user profile with the same identifier exists, its data is updated; otherwise, a
        /// new profile is created. The operation is performed asynchronously and completes when the upsert is
        /// finished.</remarks>
        /// <param name="userProfile">The user profile to insert or update. The profile's identifier determines whether an insert or update
        /// operation is performed. Cannot be null.</param>
        /// <returns></returns>
        public async Task UpsertUserProfile(UserProfile userProfile)
        {
            await _supabase.InitializeAsync();

            var user = _supabase.Auth.CurrentUser;
            if (user == null)
            {
                return;
            }

            userProfile.Id = user.Id;
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

