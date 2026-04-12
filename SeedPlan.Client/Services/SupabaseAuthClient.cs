using Supabase.Gotrue;
using System.Text.Json;
using SeedPlan.Shared.Interfaces;

namespace SeedPlan.Client.Services
{
    public class SupabaseAuthClient : IAuthClient
    {
        private readonly Supabase.Client _supabase;

        public SupabaseAuthClient(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public string? CurrentUserEmail => _supabase.Auth.CurrentUser?.Email;

        public async Task<AuthSignInResult?> SignIn(string email, string password)
        {
            var session = await _supabase.Auth.SignIn(email, password);
            if (session == null)
            {
                return null;
            }

            return new AuthSignInResult
            {
                HasUser = session.User != null,
                AccessToken = session.AccessToken,
                SessionJson = JsonSerializer.Serialize(session)
            };
        }

        public async Task<bool> SignUp(string email, string password, Dictionary<string, object> metadata)
        {
            var options = new SignUpOptions
            {
                Data = metadata
            };

            var session = await _supabase.Auth.SignUp(email, password, options);
            return session?.User != null;
        }

        public Task SignOut()
        {
            return _supabase.Auth.SignOut();
        }

        public async Task<bool> UpdateEmail(string newEmail)
        {
            var attributes = new UserAttributes { Email = newEmail };
            var user = await _supabase.Auth.Update(attributes);
            return user?.Email != null;
        }

        public async Task<bool> UpdatePassword(string newPassword)
        {
            if (CurrentUserEmail?.ToLower() == "demo@seedplan.app")
            {
                return false;
            }
            var attributes = new UserAttributes { Password = newPassword };
            var user = await _supabase.Auth.Update(attributes);
            return user != null;
        }
    }
}