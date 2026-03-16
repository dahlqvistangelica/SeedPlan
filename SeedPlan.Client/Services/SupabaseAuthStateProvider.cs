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
                    state == Constants.AuthState.TokenRefreshed ||
                    state == Constants.AuthState.UserUpdated)
                {
                    // Uppdatera tillståndet asynkront när Supabase ändras
                    NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
                }
            });
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Se till att vi bara initierar EN gång och att alla väntar på samma Task
            _initializationTask ??= InitializeInternal();
            return _initializationTask;
        }

        private async Task<AuthenticationState> InitializeInternal()
        {
            try
            {
                // Kontrollera att vi är i webbläsaren (viktigt för .NET 8)
                bool isBrowser = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Create("BROWSER"));

                if (!isBrowser)
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

                // Hämta sessionen från localStorage (WasmSessionHandler)
                await _supabase.InitializeAsync();

                // Gör ett extra försök att hämta sessionen om den inte laddades direkt
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

            var identity = new ClaimsIdentity(claims, "SupabaseAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // Metoden som dina komponenter (LoginForm/LoginDisplay) anropar
        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
        }
    }
}