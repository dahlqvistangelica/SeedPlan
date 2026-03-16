using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using Shared.Models.ViewModels;
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants;

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
        .Select("*")
        .Where(x => x.UserId == user.Id)
        .Get();

            // Logga i webbläsarens konsol (F12) för att se vad som händer
            Console.WriteLine($"DEBUG: Söker efter UID: {user.Id}");
            Console.WriteLine($"DEBUG: Antal rader returnerade från Supabase: {response.Models.Count}");

            return response.Models;
        }

        public async Task<List<SowingView>> GetMySowingViews()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<SowingView>();

            try
            {
                // Vi hämtar från vyn v_user_sowings istället för tabellen sowings
                var response = await _supabase
                    .From<SowingView>()
                    .Where(x => x.UserId == user.Id)
                    .Order("sown_date", Constants.Ordering.Descending)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid hämtning av sådder: {ex.Message}");
                return new List<SowingView>();
            }
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

            if(!newSowing.SownDate.HasValue)
            {
                throw new ArgumentException("Ett giltigt sådatum måste anges.");
            }
            if(newSowing.Quantity <= 0)
            {
                throw new ArgumentException("Du måste ange ett giltigt antal fröer du sått");
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
