using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class UserDahliaService : IUserDahliaService
    {

        private readonly Supabase.Client _supabase;
        private readonly IUserProfileService _profileService;
        private readonly IDahliaService _dahliaService;

        public UserDahliaService(Supabase.Client supabase, IUserProfileService profileService, IDahliaService dahliaService)
        {
            _supabase = supabase;
            _profileService = profileService;
            _dahliaService = dahliaService;
        }

        public async Task AddUserDahliaAsync(UserDahlia newUserDahlia)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user != null)
            {
                newUserDahlia.UserId = user.Id;
                await _supabase.From<UserDahlia>().Insert(newUserDahlia);
            }
            else
            {
                throw new Exception("Du måste vara inloggad");
            }
        }

        public async Task DeleteUserDahliaAsync(int id)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;
            await _supabase
                .From<UserDahlia>()
                .Where(x => x.Id == id)
                .Where(x => x.UserId == user.Id)
                .Delete();
        }

        public async Task<List<UserDahlia>> GetUserDahliasAsync()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null)
            {
                var session = await _supabase.Auth.RetrieveSessionAsync();
                user = session?.User;
            }

            if (user == null)
            {
                return new List<UserDahlia>();
            }

            var response = await _supabase
                .From<UserDahlia>()
                .Where(x => x.UserId == user.Id)
                .Get();
            return response.Models;
        }

        public async Task UpdateUserDahliaAsync(UserDahlia userDahlia)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null)
            {
                throw new UnauthorizedAccessException("Du måste vara inloggad för att uppdatera dahlia");
            }
            userDahlia.UserId = user.Id;

            await _supabase.From<UserDahlia>().Update(userDahlia);
        }


    }
}
