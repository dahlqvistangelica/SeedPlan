using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using SeedPlan.Client.Services;
using SeedPlan.Shared.Interfaces;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
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
            builder.Services.AddScoped<AuthService>();

            var host = builder.Build();
            var supabase = host.Services.GetRequiredService<Supabase.Client>();
            await supabase.InitializeAsync();
            var js = host.Services.GetRequiredService<IJSRuntime>();
            supabase.Auth.AddStateChangedListener(async (sender, state) =>
            {
                if (state == Supabase.Gotrue.Constants.AuthState.TokenRefreshed || state == Supabase.Gotrue.Constants.AuthState.SignedIn)
                {
                    var session = supabase.Auth.CurrentSession;
                    if (session != null)
                    {
                        var sessionJson = JsonSerializer.Serialize(session);

                        var inLocal = await js.InvokeAsync<string?>("localStorage.getItem", "sb_session");

                        if(inLocal != null)
                        { await js.InvokeVoidAsync("localStorage.setItem", "sb_session", sessionJson);
                        }
                        else
                        {
                            await js.InvokeVoidAsync("sessionStorage.setItem", "sb_session",sessionJson);
                        }
                       
                        Console.WriteLine("DEBUG: Session uppdaterad i localStorage");
                    }
                }
                else if (state == Supabase.Gotrue.Constants.AuthState.SignedOut)
                {
                    await js.InvokeVoidAsync("localStorage.removeItem", "sb_session");
                    await js.InvokeVoidAsync("sessionStorage.removeItem", "sb_session");
                    Console.WriteLine("DEBUG: Session rensad från localStorage");
                }
            });
            await host.RunAsync();
        }
    }
}