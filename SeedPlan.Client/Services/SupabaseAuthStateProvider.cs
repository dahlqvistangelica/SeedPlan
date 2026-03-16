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

            // Lyssna på ändringar (inloggning/utloggning)
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
            // Om vi inte har börjat initiera än, gör det nu. 
            // Alla anrop till denna metod kommer vänta på samma Task.
            _initializationTask ??= InitializeInternal();
            return _initializationTask;
        }

        private async Task<AuthenticationState> InitializeInternal()
        {
            try
            {
                // Kontrollera att vi faktiskt är i webbläsaren
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Create("BROWSER")))
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Vänta på att Supabase läser från localStorage
                await _supabase.InitializeAsync();

                // Om ingen session hittades direkt, gör ett sista försök att hämta den
                if (_supabase.Auth.CurrentSession == null)
                {
                    await _supabase.Auth.RetrieveSessionAsync();
                }

                return GetStateFromCurrentSession();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth Init Error: {ex.Message}");
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

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "SupabaseAuth")));
        }

        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
        }
    }
}