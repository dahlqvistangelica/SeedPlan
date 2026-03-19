using FluentResults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase.Gotrue;

namespace SeedPlan.Client.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabase;
        private readonly SupabaseAuthStateProvider _authStateProvider;
        private readonly NavigationManager _nav;

        public AuthService(Supabase.Client supabase, AuthenticationStateProvider authStateProvider, NavigationManager nav)
        {
            _supabase = supabase;
            // Vi "castar" den till vår specifika klass så att vi kommer åt NotifyUserChanged()
            _authStateProvider = (SupabaseAuthStateProvider)authStateProvider;
            _nav = nav;
        }

        public async Task<Result> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _supabase.Auth.SignIn(email, password);

                if (response?.User == null || string.IsNullOrEmpty(response.AccessToken))
                {
                    return Result.Fail("Inloggning misslyckades.");
                }

                // Inloggningen lyckades! Berätta för providern att hämta profil och uppdatera UI
                _authStateProvider.NotifyUserChanged();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(GetErrorMessage(ex.Message));
            }
        }

        public async Task<Result> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                // Här bakar vi in namnet direkt i Supabase metadata!
                var options = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "first_name", firstName },
                        { "last_name", lastName },
                        { "full_name", $"{firstName} {lastName}".Trim() }
                    }
                };

                var response = await _supabase.Auth.SignUp(email, password, options);

                if (response?.User == null)
                {
                    return Result.Fail("Registrering misslyckades.");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(GetErrorMessage(ex.Message));
            }
        }

        public async Task LogoutAsync()
        {
            await _supabase.Auth.SignOut();
            _authStateProvider.NotifyUserChanged(); // Uppdaterar skärmen direkt

            // ÄNDRING: Ta bort 'forceLoad: true'. Nu blir det en mjuk SPA-navigering utan blinkning!
            _nav.NavigateTo("/");
        }

        // Översätter Supabase kryptiska JSON-fel till användarvänlig svenska
        private string GetErrorMessage(string message)
        {
            if (message.Contains("invalid_credentials")) return "Fel e-postadress eller lösenord.";
            if (message.Contains("email_not_confirmed")) return "Du måste bekräfta din e-post innan du kan logga in.";
            if (message.Contains("user_already_exists")) return "Det finns redan ett konto med denna e-postadress.";
            if (message.Contains("weak_password")) return "Lösenordet är för svagt. Använd minst 6 tecken.";

            return "Ett oväntat fel uppstod vid inloggningen.";
        }
    }
}