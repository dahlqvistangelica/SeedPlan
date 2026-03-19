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
            
            _authStateProvider = (SupabaseAuthStateProvider)authStateProvider;
            _nav = nav;
        }
        /// <summary>
        /// Attempts to sign in a user asynchronously using the specified email address and password.
        /// </summary>
        /// <remarks>If the login is successful, the user's authentication state is updated and any
        /// registered listeners are notified. If authentication fails or an error occurs, the Result will indicate
        /// failure with an appropriate error message.</remarks>
        /// <param name="email">The email address of the user to authenticate. Cannot be null or empty.</param>
        /// <param name="password">The password associated with the specified email address. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result indicating whether the
        /// login was successful. If authentication fails, the Result contains an error message.</returns>
        public async Task<Result> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _supabase.Auth.SignIn(email, password);

                if (response?.User == null || string.IsNullOrEmpty(response.AccessToken))
                {
                    return Result.Fail("Inloggning misslyckades.");
                }
                //Login sucess, confirm and update UI.
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
                // Saving name directly in Supabase metadata.
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
            _authStateProvider.NotifyUserChanged(); // Update screen instantly.
            //Change: Removed 'forceLoad: true'. Soft renderingen without blink.
           
            _nav.NavigateTo("/");
        }

      
        /// <summary>
        /// Translates Supabase error messages into user-friendly Swedish descriptions.
        /// </summary>
        /// <remarks>This method is intended to improve the user experience by converting technical error
        /// codes from Supabase into clear, localized messages suitable for display to end users.</remarks>
        /// <param name="message">The error message received from Supabase. Must not be null.</param>
        /// <returns>A user-friendly Swedish error message corresponding to the provided Supabase error code. Returns a generic
        /// message if the error code is unrecognized.</returns>
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