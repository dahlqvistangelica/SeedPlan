using Cropper.Blazor.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using SeedPlan.Client.Services;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using Supabase;
using System.Text.Json;

namespace SeedPlan.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Configuration.AddJsonStream(await new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            }.GetStreamAsync("appsettings.json"));

            if (builder.HostEnvironment.IsDevelopment())
            {
                var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
                var response = await http.GetAsync("appsettings.Development.json");
                if (response.IsSuccessStatusCode)
                {
                    builder.Configuration.AddJsonStream(await response.Content.ReadAsStreamAsync());
                }
            }

            var supabaseUrl = builder.Configuration["SUPABASE_URL"] ?? "";
            var supabaseKey = builder.Configuration["SUPABASE_ANON_KEY"] ?? "";

            Console.WriteLine($"Connecting to Supabase: {supabaseUrl}");

            // 1. Supabase client and SessionHandler
            builder.Services.AddScoped(provider =>
            {
                return new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = false,
                    AutoConnectRealtime = false,
                    SessionHandler = new LocalStorageSessionHandler()
                });
            });

            // 2. All your database services (UserProfiles etc.)
            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();
            builder.Services.AddScoped<IUserDahliaService, UserDahliaService>();
            builder.Services.AddScoped<IDahliaService, DahliaService>();
            builder.Services.AddScoped<IFeatureService, FeatureService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<FeedbackModalService>();
            

            // 3. Blazor's built-in security
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();

            // 4. THE NEW AUTH ARCHITECTURE (Important order!)
            // A: Register the class so our engine (AuthService) can call it
            builder.Services.AddScoped<SupabaseAuthStateProvider>();

            // B: Tell Blazor (AuthorizeView etc.) to use it
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<SupabaseAuthStateProvider>());

            // C: Register our new login engine
            builder.Services.AddScoped<IAuthClient, SupabaseAuthClient>();
            builder.Services.AddScoped<IAuthStateNotifier>(sp =>
                sp.GetRequiredService<SupabaseAuthStateProvider>());
            builder.Services.AddScoped<AuthService>();

            // Register notificationservice.
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddCropper();

            //Appmode-service
            builder.Services.AddScoped<AppState>();

            var host = builder.Build();
            var supabase = host.Services.GetRequiredService<Supabase.Client>();
            var js = host.Services.GetRequiredService<IJSRuntime>();

            // Flag to ignore SignedOut during initialization
            var isInitializing = true;

            supabase.Auth.AddStateChangedListener(async (sender, state) =>
            {


                if (state == Supabase.Gotrue.Constants.AuthState.SignedIn)
                {
                    // We are now signed in — stop ignoring SignedOut
                    isInitializing = false;

                }
                else if (state == Supabase.Gotrue.Constants.AuthState.TokenRefreshed)
                {
                    isInitializing = false;
                    var session = supabase.Auth.CurrentSession;
                    if (session != null)
                    {
                        var sessionJson = JsonSerializer.Serialize(session);
                        var rememberMe = await js.InvokeAsync<string?>(
                            "localStorage.getItem", "sb_remember_me");

                        if (rememberMe == "true")
                        {
                            await js.InvokeVoidAsync("localStorage.setItem", "sb_session", sessionJson);

                        }
                        else
                        {
                            await js.InvokeVoidAsync("sessionStorage.setItem", "sb_session", sessionJson);

                        }
                    }
                }
                else if (state == Supabase.Gotrue.Constants.AuthState.SignedOut)
                {
                    if (isInitializing)
                    {

                        return;
                    }

                    await js.InvokeVoidAsync("localStorage.removeItem", "sb_session");
                    await js.InvokeVoidAsync("sessionStorage.removeItem", "sb_session");
                    await js.InvokeVoidAsync("localStorage.removeItem", "sb_remember_me");

                }
            });

            await supabase.InitializeAsync();
            // Remove Task.Delay and isInitializing = false from here

            await host.RunAsync();
        }
    }
}