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
                // Vi lyssnar fortfarande, men vi anropar en metod som inte startar nya anrop
                if (state == Constants.AuthState.SignedIn ||
                    state == Constants.AuthState.SignedOut ||
                    state == Constants.AuthState.TokenRefreshed ||
                    state == Constants.AuthState.UserUpdated)
                {
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
                    // Initiera bara EN gång. Detta läser in sessionen från localStorage.
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

            var identity = new ClaimsIdentity(claims, "SupabaseAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
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