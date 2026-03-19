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
                var js = provider.GetRequiredService<IJSRuntime>() as IJSInProcessRuntime;
                return new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false,
                    SessionHandler = new LocalStorageSessionHandler(js)
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

            await builder.Build().RunAsync();
        }
    }
}