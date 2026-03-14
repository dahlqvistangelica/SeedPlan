// SeedPlan.Client / Program.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SeedPlan.Client.Services;
using SeedPlan.Shared.Interfaces;
using Supabase; // Glöm inte denna

namespace SeedPlan.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // 1. Registrera Supabase även här (viktigt för WebAssembly!)
            // Vi hämtar inställningarna (se till att de finns i clientens wwwroot/appsettings.json eller hårdkoda för test)
            var supabaseUrl = "https://vymaxxeiosihvqklvzpw.supabase.co";
            var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZ5bWF4eGVpb3NpaHZxa2x2enB3Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzMzMjY3OTMsImV4cCI6MjA4ODkwMjc5M30.sGoh1LHXBn_3AUwEyUzXGmokh2PZbGoVL3fttTJcfb0";

            builder.Services.AddScoped(provider =>
                new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                }));

            // 2. Registrera tjänsterna
            // 2. Registrera tjänsterna
            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();
            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();

            // LÄGG TILL DESSA TRE RADER:
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();

            await builder.Build().RunAsync();
        }
    }
}