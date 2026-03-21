using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using Shared.Models;

namespace SeedPlan.Client.Services
{
    public class PlantLibraryService: IPlantLibraryService
    {
        private readonly Supabase.Client _supabase;
        private readonly IUserProfileService _profileService;

        public PlantLibraryService(Supabase.Client supabase, IUserProfileService profileService)
        {
            _supabase = supabase;
            _profileService = profileService;
        }
        /// <summary>
        /// Asynchronously searches for plants whose names contain the specified search term.
        /// </summary>
        /// <remarks>The search is limited to a maximum of 10 results. The method performs a
        /// case-insensitive search using a partial match on plant names.</remarks>
        /// <param name="searchTerm">The text to search for within plant names. The search is case-insensitive. If null, empty, or whitespace, an
        /// empty list is returned.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of plants whose names
        /// match the search term. The list is empty if no matches are found or if the search term is null, empty, or
        /// whitespace.</returns>
        public async Task<List<Plant>> SearchPlantsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Plant>();
            }
            var response = await _supabase
                .From<Plant>()
                .Filter("plant_name", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Limit(10)
                .Get();
            return response.Models;

        }

        /// <summary>
        /// Asynchronously retrieves all varieties associated with the specified plant.
        /// </summary>
        /// <param name="plantId">The unique identifier of the plant for which to retrieve varieties.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of varieties for the
        /// specified plant. The list will be empty if no varieties are found.</returns>
        public async Task<List<Variety>> GetVarietiesForPlantAsync(int plantId)
        {
            var response = await _supabase
                .From<Variety>()
                .Where(v => v.PlantId == plantId)
                .Get();
            return response.Models;
        }
        /// <summary>
        /// Asynchronously retrieves all available varieties from the data source.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all varieties. The
        /// list will be empty if no varieties are found.</returns>
        public async Task<List<Variety>> GetAllVarietiesAsync()
        {
            var response = await _supabase
                .From<Variety>()
                .Get();
            return response.Models;
        }

        /// <summary>
        /// Asynchronously retrieves all plants, ordered by plant name in ascending order.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all plants, ordered
        /// alphabetically by name. The list will be empty if no plants are found.</returns>
        public async Task<List<Plant>> GetAllPlantsAsync()
        {
            var response = await _supabase
                .From<Plant>()
                .Order(x => x.PlantName, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();
            return response.Models;

        }
        /// <summary>
        /// Asynchronously retrieves a list of plants that are suitable for sowing based on the provided last frost
        /// date.
        /// </summary>
        /// <remarks>The method calculates sowing suggestions by considering each plant's required lead
        /// time and weeks before the last frost. Only plants with sowing dates within seven days of today are included
        /// in the result.</remarks>
        /// <param name="lastFrostDate">The date of the last expected frost. Used to calculate optimal sowing times for each plant.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of plants recommended for
        /// sowing within a week of the current date.</returns>
        public async Task<List<Plant>> GetGeneralSowingSuggestionsAsync(DateTime lastFrostDate)
        {
            var allPlants = await GetAllPlantsAsync();
            var today = DateTime.Today;

            return allPlants
                .Select(plant =>
                {
                    var sowFromDate = lastFrostDate.AddDays(-(plant.SowingLeadTime * 7));
                    var deadlineDate = sowFromDate.AddDays(14);

                    return new
                    {
                        Plant = plant,
                        SowFrom = sowFromDate,
                        Deadline = deadlineDate
                    };
                })
                .Where(x => today >= x.SowFrom && today <= x.Deadline)
                .OrderBy(x => x.SowFrom)
                .Select(x => x.Plant)
                .ToList();
        }

        /// <summary>
        /// Asynchronously adds a new variety to the data store.
        /// </summary>
        /// <param name="variety">The variety to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added variety, including any
        /// data populated by the data store.</returns>
        public async Task<Variety> AddVarietyAsync(Variety variety)
        {
            var response = await _supabase
                .From<Variety>()
                .Insert(variety);
            return response.Model;
        }
        /// <summary>
        /// Generates an overview of sowing periods for all plants based on the specified last frost date.
        /// </summary>
        /// <remarks>Plants are grouped into past, current, or upcoming sowing periods based on their
        /// recommended sowing lead time and the specified last frost date. The current sowing period list is ordered by
        /// the earliest recommended sowing date.</remarks>
        /// <param name="lastFrost">The date of the most recent frost, used as a reference point to calculate sowing periods for each plant.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a SowingOverview object
        /// categorizing plants into past, current, and upcoming sowing periods relative to the provided last frost
        /// date.</returns>

        public async Task<SowingOverview> GetSowingOverviewAsync(DateTime lastFrost)
        {
            var allPlants = await GetAllPlantsAsync();
            var today = DateTime.Today;
            var overview = new SowingOverview();

            foreach(var plant in allPlants)
            {
                var start = lastFrost.AddDays(-(plant.SowingLeadTime * 7));

                var end = start.AddDays(14);
                if(today > end)
                {
                    overview.Past.Add(plant);
                }
                else if(today >= start && today <= end)
                {
                    overview.Current.Add(plant);
                }
                else if(start > today && start <= today.AddDays(14))
                {
                    overview.Upcoming.Add(plant);
                }


            }
            overview.Past = overview.Past.OrderBy(p => lastFrost.AddDays(-(p.SowingLeadTime * 7))).ToList();
            overview.Current = overview.Current.OrderBy(p => lastFrost.AddDays(-(p.SowingLeadTime * 7))).ToList();
            overview.Upcoming = overview.Upcoming.OrderBy(p => lastFrost.AddDays(-(p.SowingLeadTime * 7))).ToList();

            return overview;
        }
    }
}
