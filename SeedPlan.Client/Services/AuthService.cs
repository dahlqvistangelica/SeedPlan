using FluentResults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Supabase.Gotrue;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;

namespace SeedPlan.Client.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabase;
        private readonly SupabaseAuthStateProvider _authStateProvider;
        private readonly NavigationManager _nav;
        private readonly IJSRuntime _js;

        public AuthService(Supabase.Client supabase, AuthenticationStateProvider authStateProvider, NavigationManager nav, IJSRuntime js)
        {
            _supabase = supabase;
            _js = js;
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
        public async Task<Result> LoginAsync(string email, string password, bool rememberMe)
        {
            try
            {
                var session = await _supabase.Auth.SignIn(email, password);

                if (session?.User == null || string.IsNullOrEmpty(session.AccessToken))
                    return Result.Fail("Inloggning misslyckades.");


                if (rememberMe)
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "sb_session",
                        JsonSerializer.Serialize(session));
                    await _js.InvokeVoidAsync("localStorage.setItem", "sb_remember_me", "true");
                    await _js.InvokeVoidAsync("sessionStorage.removeItem", "sb_session");

                    // Verifiera att det sparades
                    var verify = await _js.InvokeAsync<string?>("localStorage.getItem", "sb_remember_me");
                }
                else
                {
                    await _js.InvokeVoidAsync("sessionStorage.setItem", "sb_session",
                        JsonSerializer.Serialize(session));
                    await _js.InvokeVoidAsync("localStorage.removeItem", "sb_session");
                    await _js.InvokeVoidAsync("localStorage.removeItem", "sb_remember_me");
                }

                _authStateProvider.NotifyUserChanged();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(GetErrorMessage(ex.Message));
            }
        }
        /// <summary>
        /// Registers a new user account with the specified email address, password, and personal information
        /// asynchronously.
        /// </summary>
        /// <remarks>The user's first and last name are stored as metadata during registration. If
        /// registration fails, the returned Result contains an error message describing the failure.</remarks>
        /// <param name="email">The email address to associate with the new user account. Cannot be null or empty.</param>
        /// <param name="password">The password to use for the new user account. Cannot be null or empty.</param>
        /// <param name="firstName">The first name of the user to register. Cannot be null or empty.</param>
        /// <param name="lastName">The last name of the user to register. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result indicating whether the
        /// registration was successful or failed.</returns>
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
            await _js.InvokeVoidAsync("localStorage.removeItem", "sb_session");
            await _js.InvokeVoidAsync("localStorage.removeItem", "sb_remember_me");
            await _js.InvokeVoidAsync("sessionStorage.removeItem", "sb_session");
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

        // Lägg till dessa metoder i AuthService

        public async Task<Result> UpdateEmailAsync(string newEmail)
        {
            try
            {
                var attrs = new UserAttributes { Email = newEmail };
                var response = await _supabase.Auth.Update(attrs);

                if (response?.Email == null)
                {
                    return Result.Fail("Kunde inte uppdatera e-post.");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(GetErrorMessage(ex.Message));
            }
        }

        public async Task<Result> UpdatePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                // Hämta nuvarande användare
                var user = _supabase.Auth.CurrentUser;
                if (user?.Email == null)
                {
                    return Result.Fail("Kunde inte hämta användaruppgifter.");
                }

                // Verifiera det gamla lösenordet genom att försöka logga in igen
                try
                {
                    await _supabase.Auth.SignIn(user.Email, currentPassword);
                }
                catch (Exception)
                {
                    return Result.Fail("Det gamla lösenordet är felaktigt.");
                }

                // Om verifieringen lyckas, uppdatera med det nya lösenordet
                var attrs = new UserAttributes { Password = newPassword };
                var response = await _supabase.Auth.Update(attrs);

                if (response == null)
                {
                    return Result.Fail("Kunde inte uppdatera lösenord.");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(GetErrorMessage(ex.Message));
            }
        }

        /// <summary>
        /// Kontoradering kräver service-role (server/Edge Function).
        /// </summary>
        public Task<Result> DeleteAccountAsync()
        {
            return Task.FromResult(
                Result.Fail("Kontoradering kräver en server- eller Edge Function med service-role."));
        }
    }
}