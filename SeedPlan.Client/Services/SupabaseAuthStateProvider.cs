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
            _supabase = supabaseClient;

            _supabase.Auth.AddStateChangedListener((sender, state) =>
            {
                if (state == Constants.AuthState.SignedIn ||
                    state == Constants.AuthState.SignedOut ||
                    state == Constants.AuthState.TokenRefreshed)
                {
                    NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
                }
            });
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Vi sparar ner initieringen i en Task. Alla som anropar denna metod 
            // kommer nu att vänta på att exakt samma initiering blir klar.
            _initializationTask ??= InitializeInternal();
            return _initializationTask;
        }

        private async Task<AuthenticationState> InitializeInternal()
        {
            try
            {
                // Vänta på att sessionen läses in från localStorage
                await _supabase.InitializeAsync();

                // Gör ett aktivt försök att hämta sessionen om InitializeAsync missade den
                if (_supabase.Auth.CurrentSession == null)
                {
                    await _supabase.Auth.RetrieveSessionAsync();
                }

                return GetStateFromCurrentSession();
            }
            catch
            {
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

        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
        }
    }
}