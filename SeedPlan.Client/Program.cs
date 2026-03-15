using Microsoft.AspNetCore.Components.Authorization;
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

            var supabaseUrl = builder.Configuration["SUPABASE_URL"] ?? "";
            var supabaseKey = builder.Configuration["SUPABASE_ANON_KEY"] ?? "";

            builder.Services.AddScoped(provider =>
            {
                var js = provider.GetRequiredService<IJSRuntime>() as IJSInProcessRuntime;
                return new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = false, // ÄNDRA TILL FALSE - Detta stoppar kraschen!
                    SessionHandler = new WasmSessionHandler(js)
                });
            });

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();
            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();

            await builder.Build().RunAsync();
        }
    }

    // --- Hjälparklass för att spara inloggning i webbläsaren ---
    public class WasmSessionHandler : IGotrueSessionPersistence<Session>
    {
        private readonly IJSInProcessRuntime? _js;
        public WasmSessionHandler(IJSInProcessRuntime? js) { _js = js; }
        public void SaveSession(Session session) => _js?.InvokeVoid("localStorage.setItem", "sb_session", JsonSerializer.Serialize(session));
        public void DestroySession() => _js?.InvokeVoid("localStorage.removeItem", "sb_session");
        public Session? LoadSession()
        {
            try
            {
                var json = _js?.Invoke<string>("localStorage.getItem", "sb_session");
                return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Session>(json);
            }
            catch { return null; }
        }
    }
}