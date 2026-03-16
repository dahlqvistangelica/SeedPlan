using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Supabase;
using Supabase.Gotrue;

namespace SeedPlan.Client.Services
{
    public class SupabaseAuthStateProvider : AuthenticationStateProvider
    {
        private readonly Supabase.Client _supabase;
        private bool _isInitialized = false;

        public SupabaseAuthStateProvider(Supabase.Client supabaseClient)
        {
            _supabase = supabaseClient;

            // Denna lyssnar på in/utloggning och token-uppdateringar
            _supabase.Auth.AddStateChangedListener((sender, state) =>
            {
                if (state == Constants.AuthState.SignedIn ||
                    state == Constants.AuthState.SignedOut ||
                    state == Constants.AuthState.TokenRefreshed ||
                    state == Constants.AuthState.UserUpdated)
                {
                    // Vi skickar det aktuella tillståndet direkt till Blazor
                    NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
                }
            });
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                if (!_isInitialized)
                {
                    await _supabase.InitializeAsync();
                    _isInitialized = true;
                }

                return GetStateFromCurrentSession();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth Error: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        private AuthenticationState GetStateFromCurrentSession()
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

            // "SupabaseAuth" gör att User.Identity.IsAuthenticated blir true
            var identity = new ClaimsIdentity(claims, "SupabaseAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}