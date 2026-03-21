using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using SeedPlan.Shared.Interfaces;
using System.Text.Json;
using Microsoft.JSInterop;
using Supabase.Gotrue;

namespace SeedPlan.Client.Services
{
    public class SupabaseAuthStateProvider : AuthenticationStateProvider
    {
        private readonly Supabase.Client _supabase;
        private readonly IUserProfileService _profileService;
        private readonly IJSRuntime _js;
        private AuthenticationState? _cachedState;
        private bool _initialized = false;


        public SupabaseAuthStateProvider(Supabase.Client supabase, IUserProfileService profileService, IJSRuntime js)
        {
            _supabase = supabase;
            _profileService = profileService;
            _js = js;
            
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
            if (_cachedState != null && _initialized)
            {
                Console.WriteLine("DEBUG: Returnerar cachad state.");
                return _cachedState;
            }

            try
            {
                // 2. Force read from loaclStorage before other checks
                //await _supabase.InitializeAsync();
                Console.WriteLine($"DEBUG: CurrentSession = {(_supabase.Auth.CurrentSession == null ? "NULL" : "FINNS")}");

                if (_supabase.Auth.CurrentSession == null)
                {
                    try
                    {
                        //Check sessionStorage first(no "remind me")
                        //After that check localStorage("remind me")
                        var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", "sb_session");

                        if(string.IsNullOrEmpty(json))
                        {
                            json = await _js.InvokeAsync<string?>("localStorage.getItem", "sb_session");
                        }
                        Console.WriteLine($"DEBUG: localStorage sb_session = {(string.IsNullOrEmpty(json) ? "TOMT" : json.Length + " tecken")}");

                        if (!string.IsNullOrEmpty(json))
                        {
                            var savedSession = JsonSerializer.Deserialize<Session>(json);
                            Console.WriteLine($"DEBUG: Deserialiserad session AccessToken = {(savedSession?.AccessToken == null ? "NULL" : "FINNS")}");
                            Console.WriteLine($"DEBUG: Deserialiserad session RefreshToken = {(savedSession?.RefreshToken == null ? "NULL" : "FINNS")}");
                            if (savedSession?.AccessToken != null && savedSession.RefreshToken != null)
                            {
                                try
                                {
                                    await _supabase.Auth.SetSession(
                                        savedSession.AccessToken, savedSession.RefreshToken);
                                    
                                    Console.WriteLine($"DEBUG: SetSession klar, CurrentSession = {(_supabase.Auth.CurrentSession == null ? "NULL" : "FINNS")}");
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine($"DEBUG: SetSession KRASCHADE: {ex.Message}");
                                    //Refresh token has expired - clear and continue as signed out.
                                    await _js.InvokeVoidAsync("localStorage.removeItem", "sb_session");
                                    await _js.InvokeVoidAsync("sessionStorage.removeItem", "sb_session");
                                    Console.WriteLine("Session utgången, rensar localStorage.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Kunde inte läsa session från localStorage: {ex.Message} ");
                    }
                }
                _initialized = true;
                var session = _supabase.Auth.CurrentSession;

                //Mark as initialized.
                Console.WriteLine($"DEBUG: Slutlig session = {(session?.User == null ? "INGEN ANVÄNDARE" : session.User.Email)}");
                

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

                // 5. Save and return the complete loggin.
                var identity = new ClaimsIdentity(claims, "SupabaseAuth");
                _cachedState = new AuthenticationState(new ClaimsPrincipal(identity));
                return _cachedState;
            }
            catch
            {
                _initialized = true; //Even on fault mark as initialized.
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
            _initialized = false;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); // Tells Blazor to rerender screen.
        }

        private AuthenticationState CreateAnonymousState()
        {
            _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            return _cachedState;
        }
    }
}