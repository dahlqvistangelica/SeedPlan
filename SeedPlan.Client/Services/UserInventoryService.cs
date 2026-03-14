using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;

namespace SeedPlan.Client.Services
{
    public class UserInventoryService: IUserInventoryService
    {
        private readonly Supabase.Client _supabase;
        private readonly IPlantLibraryService _plantLibrary;
        private readonly IUserProfileService _profileService;

        // Här injiceras både Supabase och en annan tjänst automatiskt
        public UserInventoryService(Supabase.Client supabase,IPlantLibraryService plantLibrary, IUserProfileService profileService)
        {
            _supabase = supabase;
            _plantLibrary = plantLibrary;
            _profileService = profileService;
        }
        //Hämta användarens fröer som är redo att sås.
        public async Task<List<Seed>> GetSeedsReadyForSowing()
        {
            var profile = await _profileService.GetUserProfile();
            if (profile?.LastFrostDate == null)
            {
                return new List<Seed>();
            }
            var allSeeds = await GetMySeeds();
            var lastFrost = profile.LastFrostDate.Value;

            return allSeeds.Where(s =>
            {
                if (s.Plant == null) { return false; }
                var targetDate = lastFrost.AddDays(-(s.Plant.SowingLeadTime * 7));
                var diff = (targetDate - DateTime.Now).TotalDays;

                return diff <= 7 && diff >= 7;
            }).ToList();
        }



        // Hämta alla fröer för den inloggade användaren
        public async Task<List<Seed>> GetMySeeds()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<Seed>();

            var response = await _supabase
                .From<Seed>()
                .Select("*, plants(*), varieties(*)")
                .Where(x => x.UserId == user.Id)
                .Get();

            return response.Models;
        }

        // Spara ett nytt frö
        public async Task AddSeed(Seed newSeed)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user != null)
            {
                newSeed.UserId = user.Id;
                await _supabase.From<Seed>().Insert(newSeed);
            }
            else
            {
                throw new Exception("Du måste vara inloggad");
            }
        }

        public async Task<List<PlantSowingView>> GetCurrentSowingCalendar()
        {
            var profile = await _profileService.GetUserProfile();
            if(profile?.LastFrostDate == null)
            {
                return new();
            }
            var suggestions = await _plantLibrary.GetGeneralSowingSuggestionsAsync(profile.LastFrostDate.Value);

            var mySeeds = await GetMySeeds();

            var calendar = suggestions.Select(p => new PlantSowingView
            {
                Plant = p,
                OwnedSeeds = mySeeds.Where(s => s.PlantId == p.Id).ToList(),
                HasSeeds = mySeeds.Any(s => s.PlantId == p.Id)
            }).ToList();

            return calendar;

        }
        public async Task UpdateSeed(Seed seed)
        {
            var user = _supabase.Auth.CurrentUser;
            if(user == null)
            {
                throw new UnauthorizedAccessException("Du måste vara inloggad för att uppdatera frö");
            }

            seed.UserId = user.Id;

            await _supabase
                .From<Seed>()
                .Where(x => x.Id == seed.Id)
                .Where(x => x.UserId == user.Id)
                .Update(seed);
        }

        public async Task DeleteSeed(int id)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;

            await _supabase
                .From<Seed>()
                .Where(x => x.Id == id)
                .Where(x => x.UserId == user.Id)
                .Delete();
        }
    }
}
