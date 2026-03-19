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
            // VIKTIGT: Här låg 'AddStateChangedListener' tidigare. 
            // Den är borttagen nu för att förhindra oändliga uppdateringsloopar!
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 1. Om vi redan har laddat in användaren denna session, returnera direkt från minnet.
            if (_cachedState != null)
            {
                return _cachedState;
            }

            try
            {
                // 2. Tvinga Supabase att läsa från LocalStorage INNAN vi går vidare
                await _supabase.InitializeAsync();

                if (_supabase.Auth.CurrentSession == null)
                {
                    try { await _supabase.Auth.RetrieveSessionAsync(); } catch { }
                }

                var session = _supabase.Auth.CurrentSession;


                if (session?.User == null)
                {
                    return CreateAnonymousState();
                }

                // 3. Bygg upp grund-identiteten (det som Supabase Auth vet)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, session.User.Id!),
                    new Claim(ClaimTypes.Email, session.User.Email ?? ""),
                    new Claim("sub", session.User.Id!)
                };

                // 4. Hämta datan från din egen 'user_profiles' tabell
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
                    // Tyst felhantering. Blir vi blockade av databasen, 
                    // så loggar vi bara in användaren ändå utan krasch.
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
        public void NotifyUserChanged()
        {
            _cachedState = null; // Rensa minnet
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); // Säg åt Blazor att rita om skärmen
        }

        private AuthenticationState CreateAnonymousState()
        {
            _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            return _cachedState;
        }
    }
}