using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class UserSowingService: IUserSowingService
    {
        private readonly Supabase.Client _supabase;
        private readonly IUserProfileService _profileService;
        public UserSowingService(Supabase.Client supabase, IUserProfileService profileService)
        {
            _supabase = supabase;
            _profileService = profileService;
        }
        // Hämtar användarens sådder
        public async Task<List<Sowing>> GetMySowings()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<Sowing>();

            // Vi tar bort "*" och skriver ut fälten explicit för att undvika dubbletter
            var response = await _supabase
                .From<Sowing>()
                .Select("id, seed_id, sown_date, status, notes, user_id, Seed:seed_id(*, Plant:plant_id(*))")
                .Where(x => x.UserId == user.Id)
                .Order(x => x.SownDate, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return response.Models;
        }
        //Skapa och lägg till ny sådd.
        public async Task AddSowing(Sowing newSowing)
        {
            var user = _supabase.Auth.CurrentUser;
            if(user == null)
            {
                throw new UnauthorizedAccessException("Du måste vara inloggad för att kunna lägga till sådd");
            }
                newSowing.UserId = user.Id;
            if(newSowing.SownDate == null || newSowing.SownDate == default(DateTime))
            {
                throw new ArgumentException("Ett giltigt sådatum måste anges.");
            }
            if(newSowing.Quantity == null || newSowing.Quantity <= 0)
            {
                throw new ArgumentException("Du måste ange antal fröer du sått");
            }
               
            await _supabase.From<Sowing>().Insert(newSowing);
            
        }
        //Uppdatera såddstatus
        public async Task UpdateSowingStatus(int id, int status)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;

            await _supabase
                .From<Sowing>()
                .Where(x => x.Id == id)
                .Where(x => x.UserId == user.Id)
                .Set(x => x.Status, status)
                .Update();
        }
        //Ta bort sådder
        public async Task DeleteSowing(int id)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;

            await _supabase
                .From<Sowing>()
                .Where(x => x.Id == id)
                .Where(x => x.UserId == user.Id)
                .Delete();
            
        }
        //Hämta antal aktiva sådder
        public async Task<int> GetActiveSowingCount()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return 0;

            var response = await _supabase
                .From<Sowing>()
                .Where(x => x.UserId == user.Id)
                .Where(x => x.Status < 7)
                .Get();
            return response.Models.Count;
        }
        //Visa sådder som behöver uppmärksamhet
        public async Task<List<Sowing>> GetSowingsNeedingAttention()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<Sowing>();

            // Hämtar allt som grott men inte planterats ut än (status 1-5)
            var response = await _supabase
                .From<Sowing>()
                .Select("*, seeds(*, plants(*))")
                .Where(x => x.UserId == user.Id)
                .Where(x => x.Status > 0)
                .Where(x => x.Status < 6)
                .Get();

            return response.Models;
        }
    }
}
