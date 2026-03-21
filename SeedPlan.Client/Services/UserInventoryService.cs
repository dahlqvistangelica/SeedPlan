using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;
using Shared.Models.ViewModels;

namespace SeedPlan.Client.Services
{
    public class UserInventoryService: IUserInventoryService
    {
        private readonly Supabase.Client _supabase;
        private readonly IPlantLibraryService _plantLibrary;
        private readonly IUserProfileService _profileService;

        public UserInventoryService(Supabase.Client supabase,IPlantLibraryService plantLibrary, IUserProfileService profileService)
        {
            _supabase = supabase;
            _plantLibrary = plantLibrary;
            _profileService = profileService;
        }
        /// <summary>
        /// Retrieves a list of seeds that are ready to be sown based on the user's last recorded frost date and each
        /// seed's sowing lead time.
        /// </summary>
        /// <remarks>This method filters the user's seeds to include only those whose recommended sowing
        /// window falls within the next seven days. The calculation is based on the sowing lead time specified in each
        /// seed's plant data and the user's last frost date. If the user's profile does not have a last frost date, no
        /// seeds will be returned.</remarks>
        /// <returns>A list of seeds that should be sown within the next week according to the user's last frost date. Returns an
        /// empty list if the user's last frost date is not available.</returns>
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
                if (s.PlantData == null) { return false; }
                var targetDate = lastFrost.AddDays(-(s.PlantData.SowingLeadTime * 7));
                var diff = (targetDate - DateTime.Now).TotalDays;

                return diff <= 7 && diff >= 7;
            }).ToList();
        }

        /// <summary>
        /// Retrieves the list of seeds associated with the currently authenticated user.
        /// </summary>
        /// <remarks>The returned list includes related plant information for each seed. This method
        /// requires that a user is currently authenticated; otherwise, it returns an empty list.</remarks>
        /// <returns>A list of <see cref="Seed"/> objects belonging to the current user. Returns an empty list if no user is
        /// authenticated or if the user has no seeds.</returns>
        public async Task<List<Seed>> GetMySeeds()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<Seed>();

            var response = await _supabase
                .From<Seed>()
                .Select("*, Plant:plant_id(*)")
                .Where(x => x.UserId == user.Id)
                .Get();

            return response.Models;
        }
        /// <summary>
        /// Retrieves the current user's seeds, grouped by plant name or as 'Övrigt' for seeds without a variety.
        /// </summary>
        /// <remarks>Seeds are grouped by plant name unless the variety is unspecified, in which case they
        /// are grouped under 'Övrigt'. The results are ordered by plant and variety names for easier
        /// browsing.</remarks>
        /// <returns>A collection of groupings, where each group contains seeds associated with a specific plant name or 'Övrigt'
        /// if the variety is not specified. Returns an empty collection if the user is not authenticated or has no
        /// seeds.</returns>
        public async Task<IEnumerable<IGrouping<string, SeedView>>> GetMySeedsGrouped()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return Enumerable.Empty<IGrouping<string, SeedView>>();

            // Vi hämtar från Vyn istället för tabellen
            var response = await _supabase
                .From<SeedView>()
                .Where(x => x.UserId == user.Id)
                .Get();

            var allSeeds = response.Models;

            return allSeeds
                .OrderBy(s => s.VarietyId == null ? "Övrigt" : s.PlantName)
        .ThenBy(s => s.VarietyName ?? s.Name)
        .GroupBy(s => s.VarietyId == null ? "Övrigt" : s.PlantName ?? "Övrigt");
        }

        /// <summary>
        /// Adds a new seed to the data store and associates it with the currently authenticated user.
        /// </summary>
        /// <remarks>The method requires that a user is currently authenticated. The UserId property of
        /// the provided seed will be overwritten with the authenticated user's ID before insertion.</remarks>
        /// <param name="newSeed">The seed to add. The seed's UserId property will be set to the ID of the currently authenticated user.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        /// <exception cref="Exception">Thrown if there is no authenticated user when attempting to add the seed.</exception>
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
        /// <summary>
        /// Retrieves the current sowing calendar for the authenticated user based on their profile and last frost date.
        /// </summary>
        /// <remarks>The returned sowing calendar is tailored to the user's last recorded frost date. If
        /// the user is not authenticated or their profile lacks a last frost date, the result will be empty. This
        /// method requires a valid user session and profile data to generate personalized sowing suggestions.</remarks>
        /// <returns>A list of sowing suggestions as <see cref="PlantSowingView"/> objects for the current user. Returns an empty
        /// list if the user is not authenticated or required profile information is missing.</returns>
        public async Task<List<PlantSowingView>> GetCurrentSowingCalendar()
        {
            var session = _supabase.Auth.CurrentSession;
            if(session == null)
            {
                session = await _supabase.Auth.RetrieveSessionAsync();
            }
            if(session?.User == null)
            {
                return new List<PlantSowingView>();
            }


            var profile = await _profileService.GetUserProfile();

            if (profile == null)
            {
                
                return new();
            }
            if (profile?.LastFrostDate == null)
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
        /// <summary>
        /// Updates the specified seed entity for the currently authenticated user.
        /// </summary>
        /// <remarks>The method associates the seed with the current user before updating. The update
        /// operation is performed asynchronously and requires the user to be authenticated.</remarks>
        /// <param name="seed">The seed entity to update. The entity's user ID will be set to the current user's ID before updating. Cannot
        /// be null.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if there is no authenticated user.</exception>
        public async Task UpdateSeed(Seed seed)
        {
            var user = _supabase.Auth.CurrentUser;
            if(user == null)
            {
                throw new UnauthorizedAccessException("Du måste vara inloggad för att uppdatera frö");
            }

            seed.UserId = user.Id;

            // Genom att köra Update direkt på objektet säkerställer du 
            // att Supabase använder PrimaryKey (Id) automatiskt.
            await _supabase.From<Seed>().Update(seed);
        }
        /// <summary>
        /// Deletes the seed with the specified identifier for the currently authenticated user.
        /// </summary>
        /// <remarks>If no user is currently authenticated, the method performs no action. Only seeds
        /// belonging to the current user are deleted.</remarks>
        /// <param name="id">The unique identifier of the seed to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
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
