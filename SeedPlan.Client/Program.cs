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

            Console.WriteLine($"Ansluter till Supabase: {supabaseUrl}");

            // 1. Supabase-klienten och SessionHandler
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

            // 2. Alla dina databastjänster (UserProfiles m.m.)
            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();

            // 3. Blazors inbyggda säkerhet
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();

            // 4. DEN NYA AUTH-ARKITEKTUREN (Viktig ordning!)
            // A: Registrera klassen så att vår motor (AuthService) kan anropa den
            builder.Services.AddScoped<SupabaseAuthStateProvider>();

            // B: Säg åt Blazor (AuthorizeView etc) att använda den
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<SupabaseAuthStateProvider>());

            // C: Registrera vår nya inloggnings-motor
            builder.Services.AddScoped<AuthService>();

            await builder.Build().RunAsync();
        }
    }

  
    
}