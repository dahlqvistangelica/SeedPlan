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

            _supabase.Auth.AddStateChangedListener((sender, state) =>
            {
                if (state == Constants.AuthState.SignedIn ||
                    state == Constants.AuthState.SignedOut ||
                    state == Constants.AuthState.TokenRefreshed)
                {
                    NotifyAuthStateChanged();
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

                var session = _supabase.Auth.CurrentSession;

                // FÖRBÄTTRING 2: Om sessionen är null, gör ett extra försök att hämta den
                if (session == null)
                {
                    session = await _supabase.Auth.RetrieveSessionAsync();
                }

                // Kontrollera att vi har både en användare och en giltig token
                if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, session.User.Email ?? ""),
                    new Claim(ClaimTypes.Email, session.User.Email ?? ""),
                    new Claim("sub", session.User.Id ?? "")
                };

                // Ange "SupabaseAuth" som autentiseringstyp för att IsAuthenticated ska bli true
                var identity = new ClaimsIdentity(claims, "SupabaseAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                // Logga gärna felet här om du har en logger
                Console.WriteLine($"Auth Error: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}