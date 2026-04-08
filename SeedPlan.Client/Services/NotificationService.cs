using Microsoft.JSInterop;
using SeedPlan.Shared.Helpers;
using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;
using Supabase.Gotrue;

namespace SeedPlan.Client.Services
{
    public class NotificationService
    {

        private readonly IJSRuntime _js;
        private readonly Supabase.Client _supabase;

        public NotificationService(IJSRuntime js, Supabase.Client supabase)
        {
            _js = js;
            _supabase = supabase;
        }

        public async Task<string> RequestPermissionAsync()
        {
            return await _js.InvokeAsync<string>("requestNotificationPermission");
        }
        public async Task<string> GetPermissionAsync()
        {
            return await _js.InvokeAsync<string>("getNotificationPermission");
        }

        public async Task SendNotificationAsync(string title, string body)
        {
            await _js.InvokeVoidAsync("sendNotification", title, body, "/icon-512.png");
        }
        public async Task SubscribeToPushAsync()
        {
            var permission = await RequestPermissionAsync();
            if (permission != "granted") return;

            var subscriptionJson = await _js.InvokeAsync<string?>("subscribeToPush");
            if (string.IsNullOrEmpty(subscriptionJson)) return;

            var session = await _supabase.Auth.RetrieveSessionAsync();
            //Save prenumeration in Supabase.
            var user = session?.User?? _supabase.Auth.CurrentUser;

            if (user == null) {
                Console.WriteLine("C# kunde inte hitta inloggad användare i NotificationService");
                return; }
            try
            {
                var existingSub = await _supabase.From<PushSubscription>().Where(x => x.UserId == user.Id).Single();
                if(existingSub != null)
                {
                    existingSub.SubscriptionJson = subscriptionJson;
                    existingSub.UpdatedAt = DateTime.UtcNow;
                    await _supabase.From<PushSubscription>().Update(existingSub);
                    Console.WriteLine("Befintlig push-prenumeration uppdaterad");
                    
                }
                else
                {
                    var newSub = new PushSubscription
                    {
                        UserId = user.Id,
                        SubscriptionJson = subscriptionJson,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _supabase.From<PushSubscription>().Insert(newSub);
                    Console.WriteLine("Ny pushprenumeration sparades i Supabase.");
                }
                var options = new Supabase.Postgrest.QueryOptions { OnConflict = "user_id" };
                
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Kunde inte spara prenumerationen i Supabase: {ex.Message}");
            }
        }
    
    
        
    
    }
}
