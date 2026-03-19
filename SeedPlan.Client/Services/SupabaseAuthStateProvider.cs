using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using SeedPlan.Shared.Interfaces;

namespace SeedPlan.Client.Services
{
    public class SupabaseAuthStateProvider : AuthenticationStateProvider
    {
        private readonly Supabase.Client _supabase;
        private readonly IUserProfileService _profileService;
        private AuthenticationState? _cachedState;

        public SupabaseAuthStateProvider(Supabase.Client supabase, IUserProfileService profileService)
        {
            _supabase = supabase;
            _profileService = profileService;
            
            // IMPORTANT: Removed 'AddStateChangedListener' to stop infinity loops. 
        }

        /// <summary>
        /// Asynchronously retrieves the current authentication state, including user identity and claims, for the
        /// application.
        /// </summary>
        /// <remarks>This method attempts to load the authentication state from memory, local storage, and
        /// the user profile service. If the user is authenticated, additional claims from the user profile are
        /// included. If authentication information cannot be retrieved, the method returns an anonymous authentication
        /// state. The returned AuthenticationState is cached for subsequent calls.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains an AuthenticationState object
        /// representing the current user's authentication and claims information. If no user is authenticated, the
        /// state represents an anonymous user.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 1. If already loaded userdata, return from memory.
            if (_cachedState != null)
            {
                return _cachedState;
            }

            try
            {
                // 2. Force read from loaclStorage before other checks
                await _supabase.InitializeAsync();

                if(_supabase.Auth.CurrentSession == null)
                {
                    try { await _supabase.Auth.RetrieveSessionAsync(); }
                    catch { }
                }
                var session = _supabase.Auth.CurrentSession;

                if (session?.User == null)
                {
                    return CreateAnonymousState();
                }

                // 3. Build baseidentity (supabase auth)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, session.User.Id!),
                    new Claim(ClaimTypes.Email, session.User.Email ?? ""),
                    new Claim("sub", session.User.Id!)
                };

                // 4. Collect data from user_profiles table. 
                try
                {
                    var profile = await _profileService.GetUserProfile();
                    if (profile != null)
                    {
                        claims.Add(new Claim("full_name", profile.FullName ?? ""));

                        if (profile.LastFrostDate.HasValue)
                            claims.Add(new Claim("frost_date", profile.LastFrostDate.Value.ToString("yyyy-MM-dd")));
                    }
                }
                catch (Exception ex)
                {
                    // Silent errorhandling. If db blocks ut sign in user anyway without crashing app.
                    Console.WriteLine($"Kunde inte hämta profil: {ex.Message}");
                }

                // 5. Spara och returnera den färdiga inloggningen
                var identity = new ClaimsIdentity(claims, "SupabaseAuth");
                _cachedState = new AuthenticationState(new ClaimsPrincipal(identity));
                return _cachedState;
            }
            catch
            {
                return CreateAnonymousState();
            }
        }

        // Metod som anropas manuellt när användaren loggar in eller ut
        /// <summary>
        /// Notifies the authentication state provider that the current user's authentication state has changed.
        /// </summary>
        /// <remarks>Call this method after a user logs in or out to ensure that components depending on
        /// authentication state are updated. This triggers a re-evaluation of the authentication state and causes the
        /// UI to refresh as needed.</remarks>
        public void NotifyUserChanged()
        {
            _cachedState = null; // Clear memory
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); // Tells Blazor to rerender screen.
        }

        private AuthenticationState CreateAnonymousState()
        {
            _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            return _cachedState;
        }
    }
}