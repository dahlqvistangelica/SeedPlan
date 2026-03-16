using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using SeedPlan.Client.Services;
using SeedPlan.Components;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
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

            // --- 1. KONFIGURATION & MILJÖ ---
            var supabaseUrl = builder.Configuration["SUPABASE_URL"];
            var supabaseKey = builder.Configuration["SUPABASE_ANON_KEY"];

            // --- 2. INFRASTRUKTUR & PROXY (Railway/Docker fix) ---
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // --- 3. SUPABASE KONFIGURATION ---
            // Scoped klient för server-side operationer
            builder.Services.AddScoped(provider =>
                new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = false, // Måste vara false på servern
                    AutoConnectRealtime = false, // Måste vara false på servern
                    SessionHandler = new ServerSessionHandler()
                }));

            // --- 4. DOMÄNTJÄNSTER (Business Logic) ---
            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();

            // --- 5. AUTENTISERING & IDENTITET ---
            builder.Services.AddAuthentication("SupabaseAuth")
                .AddCookie("SupabaseAuth", options =>
                {
                    options.Cookie.Name = "SeedPlanAuth";
                    options.Cookie.MaxAge = TimeSpan.FromDays(30);
                    options.Cookie.IsEssential = true;
                    options.SlidingExpiration = true;

                    options.Events.OnRedirectToLogin = context =>
                    {
                        // Blockera API-anrop med 401, tillåt vanliga sidladdningar för WASM-hantering
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = 401;
                        }
                        return Task.CompletedTask;
                    };
                });

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();

            // --- 6. UI & KOMPONENTER ---
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            var app = builder.Build();

            // --- 7. MIDDLEWARE PIPELINE (Ordningen är viktig!) ---

            // Hantera headers från Railway/Proxy först
            app.UseForwardedHeaders();

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

            // Konfigurera statiska filer och PWA-manifest
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypeProvider
            });

            app.UseAntiforgery();

            // Säkerhet
            app.UseAuthentication();
            app.UseAuthorization();

            // Mappa komponenter och render-modes
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(SeedPlan.Client._Imports).Assembly);

            app.Run();
        }
    }

    /// <summary>
    /// Hanterar sessioner på serversidan för Supabase.
    /// </summary>
    public class ServerSessionHandler : IGotrueSessionPersistence<Session>
    {
        private Session? _session;
        public void SaveSession(Session session) => _session = session;
        public void DestroySession() => _session = null;
        public Session? LoadSession() => _session;
    }
}