using SeedPlan.Shared.Helpers;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants;

namespace SeedPlan.Client.Services
{
    public class UserSowingService : IUserSowingService
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
        /// 
        public async Task UpdateSowingStatus(int id, int status)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;

            await UpdateSowingStatusAsync(new UpdateSowingStatusRequest
            {
                SowingId = id,
                TargetStatus = status,
                EventDate = DateTime.Now,
            });
        }
        public async Task UpdateSowingStatusAsync(UpdateSowingStatusRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("You have to be logged in to edit sowings.");
            }

            var sowing = await GetOwnedSowingAsync(request.SowingId, userId);

            if (!SowingStatusFlow.CanTransition(sowing.Status, request.TargetStatus))
            {
                throw new InvalidOperationException($"Invalid status transition: {sowing.Status} -> {request.TargetStatus}.");
            }

            ValidateRequestForTargetStatus(request);
            request.EventDate ??= DateTime.Now;

            try
            {
                await _supabase.Rpc("update_sowing_status_with_event", new
                {
                    p_sowing_id = request.SowingId,
                    p_target_status = request.TargetStatus,
                    p_event_date = request.EventDate?.Date,
                    p_seedlings_count = request.SeedlingCount,
                    p_harvest_weight_g = request.HarvestWeightG,
                    p_harvest_count = request.HarvestCount,
                    p_notes = request.Notes
                });
            }
            catch (Exception ex) when (
                ex.Message.Contains("transition", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid status transition.", ex);
            }
        }

        public async Task UpdateSowingProgressAsync(UpdateSowingProgressRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("You have to be logged in to edit sowings.");
            }

            if (request.ActiveSeedlingCount.HasValue && request.ActiveSeedlingCount.Value < 0)
            {
                throw new ArgumentException("Aktivt antal plantor kan inte vara negativt.");
            }

            if (!request.ActiveSeedlingCount.HasValue && string.IsNullOrWhiteSpace(request.Notes))
            {
                throw new ArgumentException("Lägg till en anteckning eller ange aktivt antal plantor.");
            }

            var sowing = await GetOwnedSowingAsync(request.SowingId, userId);

            if (request.ActiveSeedlingCount.HasValue)
            {
                sowing.Quantity = request.ActiveSeedlingCount.Value;
                await _supabase.From<Sowing>().Update(sowing);
            }

            var progressEvent = new SowingEvent
            {
                SowingId = request.SowingId,
                UserId = userId,
                EventType = "progress_note",
                EventDate = request.EventDate?.Date ?? DateTime.Today,
                SeedlingsCount = request.ActiveSeedlingCount,
                Notes = request.Notes
            };

            await _supabase.From<SowingEvent>().Insert(progressEvent);
        }

        private async Task<Sowing> GetOwnedSowingAsync(int sowingId, string userId)
        {
            var response = await _supabase
                .From<Sowing>()
                .Where(x => x.Id == sowingId)
                .Where(x => x.UserId == userId)
                .Limit(1)
                .Get();

            var sowing = response.Models.FirstOrDefault();
            if (sowing == null)
            {
                throw new KeyNotFoundException("Sowing not found.");
            }

            return sowing;
        }

        private static void ValidateRequestForTargetStatus(UpdateSowingStatusRequest request)
        {
            if (request.TargetStatus == (int)SowingStatus.Germinated &&
                (!request.SeedlingCount.HasValue || request.SeedlingCount.Value <= 0))
            {
                throw new ArgumentException("Seedling count is required when moving to Germinated.");
            }

            if (request.TargetStatus == (int)SowingStatus.Harvested)
            {
                var hasWeight = request.HarvestWeightG.HasValue && request.HarvestWeightG.Value > 0;
                var hasCount = request.HarvestCount.HasValue && request.HarvestCount.Value > 0;

                if (!hasWeight && !hasCount)
                {
                    throw new ArgumentException("Harvest weight or harvest count is required when moving to Harvested.");
                }
            }

            // Notes are optional for Failed status according to current product spec.
        }

        /// <summary>
        /// Deletes the sowing record with the specified identifier for the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// If there is no authenticated user, the method does not perform any operation.
        /// Only sowing records belonging to the current user are affected.
        /// </remarks>
        /// <param name="id">The unique identifier of the sowing record to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public async Task DeleteSowing(int id)
        {
            await DeleteSowingWithResult(id);
        }

        public async Task<DeleteSowingResult> DeleteSowingWithResult(int id)
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("You have to be logged in to delete sowings.");
            }

            var sowing = await GetOwnedSowingAsync(id, userId);
            var result = new DeleteSowingResult();

            if (SowingDeletionRules.ShouldReturnSeedsToInventory(sowing.Status))
            {
                var seedResponse = await _supabase
                    .From<Seed>()
                    .Where(x => x.Id == sowing.SeedId)
                    .Where(x => x.UserId == userId)
                    .Limit(1)
                    .Get();

                var seed = seedResponse.Models.FirstOrDefault();
                if (seed == null)
                {
                    throw new KeyNotFoundException("Related seed not found for sowing deletion.");
                }

                seed.Quantity += sowing.Quantity;
                await _supabase.From<Seed>().Update(seed);

                result.SeedsReturnedToInventory = true;
                result.ReturnedQuantity = sowing.Quantity;
            }

            await _supabase
                .From<Sowing>()
                .Where(x => x.Id == id)
                .Where(x => x.UserId == userId)
                .Delete();

            return result;
        }

        /// <summary>
        /// Asynchronously retrieves the number of active sowing records for the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the count of active sowing records for the current user.
        /// </returns>
        public async Task<int> GetActiveSowingCount()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return 0;

            var response = await _supabase
                .From<Sowing>()
                .Where(x => x.UserId == user.Id)
                .Where(x => x.Status <= (int)SowingStatus.Harvested)
                .Get();

            return response.Models.Count;
        }

        /// <summary>
        /// Retrieves sowings for the current user that are in active growth stages and may need attention.
        /// </summary>
        /// <returns>
        /// A list of sowings in active stages.
        /// Returns an empty list if the user is not authenticated or if no sowings are found.
        /// </returns>
        public async Task<List<Sowing>> GetSowingsNeedingAttention()
        {
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return new List<Sowing>();

            var response = await _supabase
                .From<Sowing>()
                .Select("*, seeds(*, plants(*))")
                .Where(x => x.UserId == user.Id)
                .Where(x => x.Status > (int)SowingStatus.Sown)
                .Where(x => x.Status <= (int)SowingStatus.Harvested)
                .Get();

            return response.Models;
        }

        public async Task<List<SowingEvent>> GetSowingEventsAsync(int sowingId)
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new List<SowingEvent>();
            }

            var response = await _supabase
                .From<SowingEvent>()
                .Where(x => x.SowingId == sowingId)
                .Where(x => x.UserId == userId)
                .Order("event_date", Constants.Ordering.Ascending)
                .Order("created_at", Constants.Ordering.Ascending)
                .Get();

            return response.Models;
        }

        public async Task<int> GetNextBatchNumberAsync(int seedId)
        {
            var userId = _supabase.Auth.CurrentUser?.Id;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return 1;
            }

            var response = await _supabase
                .From<Sowing>()
                .Select("batch_number")
                .Where(x => x.SeedId == seedId)
                .Where(x => x.UserId == userId)
                .Get();

            return SowingBatchNumberHelper.GetNextBatchNumber(response.Models.Select(x => x.BatchNumber));
        }
    }
}
