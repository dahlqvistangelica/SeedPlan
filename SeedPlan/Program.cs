using Microsoft.AspNetCore.Components.Authorization;
using SeedPlan.Components;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using SeedPlan.Client.Services;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace SeedPlan
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var supabaseUrl = builder.Configuration["SUPABASE_URL"];
            var supabaseKey = builder.Configuration["SUPABASE_ANON_KEY"];

            // HÄR LIGGER FIXEN SOM STOPPAR KRASCHEN
            builder.Services.AddScoped(provider =>
                new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = false, // VIKTIGT: Måste vara false på servern
                    AutoConnectRealtime = false, // VIKTIGT: Måste vara false på servern
                    SessionHandler = new ServerSessionHandler()
                }));

            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();

            builder.Services.AddAuthentication("SupabaseAuth")
                .AddCookie("SupabaseAuth", options =>
                {
                    options.Cookie.Name = "SeedPlanAuth";
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAntiforgery();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(SeedPlan.Client._Imports).Assembly);

            app.Run();
        }
    }

    public class ServerSessionHandler : IGotrueSessionPersistence<Session>
    {
        private Session? _session;
        public void SaveSession(Session session) => _session = session;
        public void DestroySession() => _session = null;
        public Session? LoadSession() => _session;
    }
}