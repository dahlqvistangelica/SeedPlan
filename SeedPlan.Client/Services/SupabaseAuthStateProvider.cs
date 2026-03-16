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

                var session = await _supabase.Auth.RetrieveSessionAsync();

                if(session?.User == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                var identity = new ClaimsIdentity(CreateClaims(session.User), "SupabaseAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));

            }
            catch (Exception ex)
            {
                // Logga gärna felet här om du har en logger
                Console.WriteLine($"Auth Error: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        private IEnumerable<Claim> CreateClaims(User user)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("sub", user.Id ?? "")
            };
        }
        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}