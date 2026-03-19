using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Supabase;
using Supabase.Gotrue;

namespace SeedPlan.Client.Services
{
    public class SupabaseAuthStateProvider : AuthenticationStateProvider
    {
        private readonly Supabase.Client _supabase;
        private Task<AuthenticationState>? _initializationTask;

        public SupabaseAuthStateProvider(Supabase.Client supabaseClient)
        {
<<<<<<< Updated upstream
            _supabase = supabaseClient;

            _supabase.Auth.AddStateChangedListener((sender, state) =>
=======
            _supabase = supabase;
            _profileService = profileService;
            //IMPORTANT: We removed 'AddStateChangedListener' to stop infitity updateloop.

        }
        /// <summary>
        /// Asynchronously retrieves the current user's authentication state, including claims from both the
        /// authentication provider and the user profile service.
        /// </summary>
        /// <remarks>This method combines authentication information from the underlying authentication
        /// provider and additional user profile data, if available. If the user profile cannot be retrieved,
        /// authentication proceeds with available claims. The result is cached for the session to improve
        /// performance.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an AuthenticationState object
        /// representing the current user's authentication state. If the user is not authenticated, the state represents
        /// an anonymous user.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //If user is already loaded, return from memory.
            
            if (_cachedState != null)
>>>>>>> Stashed changes
            {
                if (state == Constants.AuthState.SignedIn ||
                    state == Constants.AuthState.SignedOut ||
                    state == Constants.AuthState.TokenRefreshed)
                {
                    NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
                }
            });
        }
        /// <summary>
        /// Asynchronously retrieves the current authentication state for the user.
        /// </summary>
        /// <remarks>This method may cache the authentication state to improve performance. Subsequent
        /// calls may return the same result until the authentication state changes.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the current <see
        /// cref="AuthenticationState"/>.</returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            _initializationTask ??= InitializeInternal();
            return _initializationTask;
        }
        /// <summary>
        /// Initializes the authentication state by ensuring the session is loaded and returns the current
        /// authentication state.
        /// </summary>
        /// <remarks>This method attempts to load the authentication session from storage and retrieve it
        /// if not already available. If an error occurs during initialization, an unauthenticated state is
        /// returned.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the current authentication state
        /// based on the loaded session, or an unauthenticated state if initialization fails.</returns>
        private async Task<AuthenticationState> InitializeInternal()
        {
            try
            {
<<<<<<< Updated upstream
                // Vänta på att sessionen läses in från localStorage
=======
                // Force check of LocalStorage before anything else. 
>>>>>>> Stashed changes
                await _supabase.InitializeAsync();

                // Gör ett aktivt försök att hämta sessionen om InitializeAsync missade den
                if (_supabase.Auth.CurrentSession == null)
                {
                    await _supabase.Auth.RetrieveSessionAsync();
                }

<<<<<<< Updated upstream
                return GetStateFromCurrentSession();
=======
                var session = _supabase.Auth.CurrentSession;


                if (session?.User == null)
                {
                    return CreateAnonymousState();
                }

                // 3. Build baseidientity (Supabase Auth model)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, session.User.Id!),
                    new Claim(ClaimTypes.Email, session.User.Email ?? ""),
                    new Claim("sub", session.User.Id!)
                };

                // 4. Collect data from user_profiles table
                try
                {
                    var profile = await _profileService.GetUserProfile();
                    if (profile != null)
                    {
                        claims.Add(new Claim("full_name", profile.FullName ?? ""));
                            claims.Add(new Claim("growing_zone", profile.GrowingZone.ToString()));

                        if (profile.LastFrostDate.HasValue)
                            claims.Add(new Claim("frost_date", profile.LastFrostDate.Value.ToString("yyyy-MM-dd")));
                    }
                }
                catch (Exception ex)
                {
                    //Silent errorhandling, if db blocks us, the user is still signed in without crashing.
                    
                    Console.WriteLine($"Kunde inte hämta profil: {ex.Message}");
                }

                // 5. Save and return the finished login.
                var identity = new ClaimsIdentity(claims, "SupabaseAuth");
                _cachedState = new AuthenticationState(new ClaimsPrincipal(identity));
                return _cachedState;
>>>>>>> Stashed changes
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
<<<<<<< Updated upstream
        /// <summary>
        /// Retrieves the current authentication state based on the active Supabase session.
        /// </summary>
        /// <remarks>This method constructs a ClaimsPrincipal using information from the Supabase session.
        /// If the session or user information is missing, the returned AuthenticationState will indicate an
        /// unauthenticated user.</remarks>
        /// <returns>An AuthenticationState representing the current user if a valid session exists; otherwise, an
        /// unauthenticated state with an empty ClaimsPrincipal.</returns>
        private AuthenticationState GetStateFromCurrentSession()
=======


        /// <summary>
        /// Notifies the authentication state provider that the current user's authentication state has changed.
        /// </summary>
        /// <remarks>Call this method after a user logs in or out to ensure that components depending on
        /// authentication state are updated. This triggers a re-evaluation of the authentication state and causes the
        /// UI to refresh as needed.</remarks>
        public void NotifyUserChanged()
>>>>>>> Stashed changes
        {
            var session = _supabase.Auth.CurrentSession;
            if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, session.User.Email ?? ""),
            new Claim(ClaimTypes.Email, session.User.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, session.User.Id ?? ""),
            new Claim("sub", session.User.Id ?? "")
        };

            var identity = new ClaimsIdentity(claims, "SupabaseAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        /// <summary>
        /// Notifies subscribers that the authentication state has changed.
        /// </summary>
        /// <remarks>Call this method to trigger an update to all components or services that are
        /// observing authentication state changes. This is typically used after a sign-in, sign-out, or other
        /// authentication event to ensure that dependent components receive the latest authentication
        /// information.</remarks>
        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
        }
    }
}