using Cropper.Blazor.Extensions;
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
using System.Text;

namespace SeedPlan
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- 1. CONFIGURATION & ENVIRONMENT ---
            var supabaseUrl = builder.Configuration["SUPABASE_URL"];
            var supabaseKey = builder.Configuration["SUPABASE_ANON_KEY"];

            // --- 2. INFRASTRUCTURE & PROXY (Railway/Docker fix) ---
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // --- 3. SUPABASE CONFIGURATION ---
            // Scoped client for server-side operations
            builder.Services.AddScoped(provider =>
                new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = false, // Must be false on the server
                    AutoConnectRealtime = false, // Must be false on the server
                    SessionHandler = new ServerSessionHandler()
                }));

            // --- 4. DOMAIN SERVICES (Business Logic) ---
            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();
            // 4. THE NEW AUTH ARCHITECTURE (Kopierat från Client)
            builder.Services.AddScoped<SupabaseAuthStateProvider>();

            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<SupabaseAuthStateProvider>());

            builder.Services.AddScoped<IAuthClient, SupabaseAuthClient>();

            builder.Services.AddScoped<IAuthStateNotifier>(sp =>
                sp.GetRequiredService<SupabaseAuthStateProvider>());

            builder.Services.AddScoped<AuthService>();

            // Du behöver säkert dessa två också på servern om du använder prerendering:
            builder.Services.AddScoped<FeedbackModalService>();
            builder.Services.AddScoped<NotificationService>();

            // --- 5. AUTHENTICATION & IDENTITY ---
            builder.Services.AddAuthentication("SupabaseAuth")
                .AddCookie("SupabaseAuth", options =>
                {
                    options.Cookie.Name = "SeedPlanAuth";
                    options.Cookie.MaxAge = TimeSpan.FromDays(30);
                    options.Cookie.IsEssential = true;
                    options.SlidingExpiration = true;

                    options.Events.OnRedirectToLogin = context =>
                    {
                        // Block API requests with 401, allow normal page loads for WASM handling
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
            builder.Services.AddScoped(sp =>
            {
                var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
                return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
            });
            // --- 6. UI & COMPONENTS ---
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddCropper();
            var app = builder.Build();

            // --- 7. MIDDLEWARE PIPELINE (Order is important!) ---
            if (app.Configuration["MAINTENANCE_MODE"] == "true")
            {
                app.Run(async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.SendFileAsync("wwwroot/maintenance.html");
                });
                app.Run();
                return;
            }

            // Handle headers from Railway/Proxy first
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

            // Configure static files and PWA manifest
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypeProvider
            });

            app.UseAntiforgery();

            // Security
            app.UseAuthentication();
            app.UseAuthorization();

            // Sitemap

            app.MapGet("/sitemap.xml", async (IPlantLibraryService plantService) =>
            {
                var plants = await plantService.GetAllPlantsAsync();

                var xml = new StringBuilder();
                xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

                // Startsida
                xml.AppendLine("<url>");
                xml.AppendLine($"<loc>https://seedplan.se/</loc>");
                xml.AppendLine($"<lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
                xml.AppendLine("<priority>1.0</priority>");
                xml.AppendLine("</url>");

                // Guide
                xml.AppendLine("<url>");
                xml.AppendLine($"<loc>https://seedplan.se/guide</loc>");
                xml.AppendLine("<priority>0.8</priority>");
                xml.AppendLine("</url>");

                // Dynamiska växtsidor (om du lägger till dessa)
                foreach (var plant in plants.Take(100))
                {
                    xml.AppendLine("<url>");
                    xml.AppendLine($"<loc>https://seedplan.se/guide/{SlugifyPlantName(plant.PlantName)}</loc>");
                    xml.AppendLine("<priority>0.6</priority>");
                    xml.AppendLine("</url>");
                }

                xml.AppendLine("</urlset>");

                return Results.Content(xml.ToString(), "application/xml");
            });

            string SlugifyPlantName(string name) =>
                name.ToLower()
                    .Replace("å", "a").Replace("ä", "a").Replace("ö", "o")
                    .Replace(" ", "-");


            // Map components and render modes
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(SeedPlan.Client._Imports).Assembly);

            app.Run();
        }
    }

    /// <summary>
    /// Handles server-side sessions for Supabase.
    /// </summary>
    public class ServerSessionHandler : IGotrueSessionPersistence<Session>
    {
        private Session? _session;
        public void SaveSession(Session session) => _session = session;
        public void DestroySession() => _session = null;
        public Session? LoadSession() => _session;
    }
}