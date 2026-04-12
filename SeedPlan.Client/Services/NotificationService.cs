using Microsoft.JSInterop;
using SeedPlan.Shared.Models;

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
            var user = session?.User ?? _supabase.Auth.CurrentUser;

            if (user == null) return;

            try
            {
                
                var response = await _supabase.From<PushSubscription>()
                .Where(x => x.UserId == user.Id)
                .Get();

                
                var existingSub = response.Models.FirstOrDefault(x => x.SubscriptionJson == subscriptionJson);

                
                if (existingSub == null)
                {
                    var newSub = new PushSubscription
                    {
                        UserId = user.Id,
                        SubscriptionJson = subscriptionJson
                    };

                    // 
                    await _supabase.From<PushSubscription>().Insert(newSub);
                }
                else
                {
                    
                    existingSub.UpdatedAt = DateTime.UtcNow;
                    await _supabase.From<PushSubscription>().Update(existingSub);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kunde inte spara prenumeration: {ex.Message}");
            }
        }

        public async Task UnsubscribeFromPushAsync()
        {
            // Hämta denna enhets unika nyckel
            var subscriptionJson = await _js.InvokeAsync<string?>("subscribeToPush");
            if (string.IsNullOrEmpty(subscriptionJson)) return;

            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;

            try
            {

                var response = await _supabase.From<PushSubscription>()
                .Where(x => x.UserId == user.Id)
                .Get();

                var existingSub = response.Models.FirstOrDefault(x => x.SubscriptionJson == subscriptionJson);

                if (existingSub != null)
                {
                    await _supabase.From<PushSubscription>().Delete(existingSub);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kunde inte radera prenumeration: {ex.Message}");
            }
        }


    }
}
