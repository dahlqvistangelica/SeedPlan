using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Supabase;

namespace SeedPlan.Client.Services
{
    public class SupabaseAuthStateProvider : AuthenticationStateProvider
    {
        private readonly Supabase.Client _supabase;
        private bool _isInitialized = false; // Lägg till detta för att inte läsa in filen i onödan

        public SupabaseAuthStateProvider(Supabase.Client supabaseClient)
        {
            _supabase = supabaseClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // HÄR ÄR FIXEN: Läs in sessionen från webbläsaren om det är första gången
                if (!_isInitialized)
                {
                    await _supabase.InitializeAsync();
                    _isInitialized = true;
                }

                var session = _supabase.Auth.CurrentSession;

                if (session?.User == null)
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, session.User.Email ?? ""),
                    new Claim("sub", session.User.Id ?? "")
                };

                var identity = new ClaimsIdentity(claims, "SupabaseAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}