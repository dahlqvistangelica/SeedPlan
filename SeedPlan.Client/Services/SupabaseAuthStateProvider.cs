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
        /// <summary>
        /// Asynchronously retrieves the current authentication state for the user.
        /// </summary>
        /// <remarks>This method may cache the authentication state to improve performance. Subsequent
        /// calls may return the same result until the authentication state changes.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the current <see
        /// cref="AuthenticationState"/>.</returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {

            _initializationTask ??= InitializeInternal();
            return _initializationTask;
        }
        /// <summary>
        /// Initializes the authentication state by ensuring the session is loaded and returns the current
        /// authentication state.
        /// </summary>
        /// <remarks>This method attempts to load the authentication session from storage and retrieve it
        /// if not already available. If an error occurs during initialization, an unauthenticated state is
        /// returned.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the current authentication state
        /// based on the loaded session, or an unauthenticated state if initialization fails.</returns>
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
        /// <summary>
        /// Retrieves the current authentication state based on the active Supabase session.
        /// </summary>
        /// <remarks>This method constructs a ClaimsPrincipal using information from the Supabase session.
        /// If the session or user information is missing, the returned AuthenticationState will indicate an
        /// unauthenticated user.</remarks>
        /// <returns>An AuthenticationState representing the current user if a valid session exists; otherwise, an
        /// unauthenticated state with an empty ClaimsPrincipal.</returns>
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
        /// <summary>
        /// Notifies subscribers that the authentication state has changed.
        /// </summary>
        /// <remarks>Call this method to trigger an update to all components or services that are
        /// observing authentication state changes. This is typically used after a sign-in, sign-out, or other
        /// authentication event to ensure that dependent components receive the latest authentication
        /// information.</remarks>
        public void NotifyAuthStateChanged()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(GetStateFromCurrentSession()));
        }
    }
}