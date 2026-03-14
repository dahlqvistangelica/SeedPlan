using Microsoft.AspNetCore.Components.Authorization;
using SeedPlan.Components;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using SeedPlan.Client.Services;
using Supabase;

namespace SeedPlan
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Hämta inställningar
            var supabaseUrl = builder.Configuration["SUPABASE_URL"];
            var supabaseKey = builder.Configuration["SUPABASE_ANON_KEY"];

            // 2. Registrera Supabase
            builder.Services.AddSingleton(provider =>
                new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true
                }));

            // 3. Registrera egna tjänster
            builder.Services.AddScoped<IPlantLibraryService, PlantLibraryService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();
            builder.Services.AddScoped<IUserSowingService, UserSowingService>();

            // 4. Konfigurera Autentisering
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
            // 4. Lägg till ASP.NET Core-autentisering (SAKNANADES TIDIGARE)

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            // Registrera din AuthProvider (finns i Client-projektet)
            builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();

            // 5. Lägg till Blazor-komponenter och interaktivitet
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            var app = builder.Build();

            // 6. Konfigurera HTTP-pipelinen
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
            app.UseAntiforgery(); // Viktig för Blazor Form-hantering

            // Aktivera Auth-pipelinen (Måste ligga före MapRazorComponents)
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
}