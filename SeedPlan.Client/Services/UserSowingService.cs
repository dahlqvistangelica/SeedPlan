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
        /// <summary>
        /// Retrieves a list of sowings associated with the currently authenticated user.
        /// </summary>
        /// <remarks>This method requires that a user is currently authenticated. If called when no user
        /// is signed in, the result will be an empty list. The returned list contains only sowings created by the
        /// current user.</remarks>
        /// <returns>A list of <see cref="Sowing"/> objects belonging to the current user. Returns an empty list if no user is
        /// authenticated or if no sowings are found.</returns>
        public async Task<List<Sowing>> GetMySowings()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<Sowing>();

            
            var response = await _supabase
        .From<Sowing>()
        .Select("*")
        .Where(x => x.UserId == user.Id)
        .Get();

            return response.Models;
        }
        /// <summary>
        /// Retrieves a list of sowing views associated with the currently authenticated user.
        /// </summary>
        /// <remarks>The returned list is ordered by the sowing date in descending order. If no user is
        /// authenticated, or if an error occurs while fetching data, the method returns an empty list instead of
        /// throwing an exception.</remarks>
        /// <returns>A list of <see cref="SowingView"/> objects representing the user's sowings. Returns an empty list if the
        /// user is not authenticated or if an error occurs during retrieval.</returns>
        public async Task<List<SowingView>> GetMySowingViews()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<SowingView>();

            try
            {
                
                var response = await _supabase
                    .From<SowingView>()
                    .Where(x => x.UserId == user.Id)
                    .Order("sown_date", Constants.Ordering.Descending)
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when collecting sowings: {ex.Message}");
                return new List<SowingView>();
            }
        }

        /// <summary>
        /// Adds a new sowing record for the currently authenticated user.
        /// </summary>
        /// <param name="newSowing">The sowing information to add. The object must have a valid sowing date and a quantity greater than zero.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if there is no authenticated user.</exception>
        /// <exception cref="ArgumentException">Thrown if the sowing date is not set or if the quantity is less than or equal to zero.</exception>
        public async Task AddSowing(Sowing newSowing)
        {
            var user = _supabase.Auth.CurrentUser;
            if(user == null)
            {
                throw new UnauthorizedAccessException("You have to be loggen in to add a sowing");
            }

                newSowing.UserId = user.Id;

            if(!newSowing.SownDate.HasValue)
            {
                throw new ArgumentException("A valid sowingdate must be set.");
            }
            if(newSowing.Quantity <= 0)
            {
                throw new ArgumentException("You must enter a valid number of seeds you sown");
            }
               
            await _supabase.From<Sowing>().Insert(newSowing);
            
        }
        /// <summary>
        /// Updates the status of a sowing record for the currently authenticated user.
        /// </summary>
        /// <remarks>The update is performed only if a user is currently authenticated. No action is taken
        /// if the user is not authenticated.</remarks>
        /// <param name="id">The unique identifier of the sowing record to update.</param>
        /// <param name="status">The new status value to assign to the sowing record.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
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
        /// <summary>
        /// Deletes the sowing record with the specified identifier for the currently authenticated user.
        /// </summary>
        /// <remarks>If there is no authenticated user, the method does not perform any operation. Only
        /// sowing records belonging to the current user are affected.</remarks>
        /// <param name="id">The unique identifier of the sowing record to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
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
        /// <summary>
        /// Asynchronously retrieves the number of active sowing records for the currently authenticated user.
        /// </summary>
        /// <remarks>A sowing is considered active if its status is less than 7. This method requires a
        /// user to be authenticated; otherwise, it returns 0.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of active sowing
        /// records for the current user. Returns 0 if no user is authenticated or if there are no active sowings.</returns>
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
        /// <summary>
        /// Retrieves a list of sowings for the current user that have germinated but have not yet been planted out.
        /// </summary>
        /// <remarks>Only sowings with a status between 1 and 5 (inclusive) are included. The method
        /// requires the user to be authenticated; otherwise, no results are returned.</remarks>
        /// <returns>A list of sowings with a status indicating they require attention. Returns an empty list if the user is not
        /// authenticated or if no such sowings exist.</returns>
        public async Task<List<Sowing>> GetSowingsNeedingAttention()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<Sowing>();

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
