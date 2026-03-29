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


            //Save prenumeration in Supabase.
            var user = _supabase.Auth.CurrentUser;
            if (user == null) return;

            await _supabase.From<PushSubscription>().Upsert(new PushSubscription
            {
                UserId = user.Id,
                SubscriptionJson = subscriptionJson,
                UpdatedAt = DateTime.Now
            });

            Console.WriteLine("Push-prenumeration sparad i Supabase"); 
        }
        public async Task CheckAndNotifyStaleAsync(List<SowingView> sowings)
        {
            var permission = await GetPermissionAsync();
            if (permission != "granted") return;

            foreach(var sowing in sowings)
            {
                var warning = SowingHelper.GetStaleWarning(
                    sowing.Status,
                    sowing.StatusUpdatedAt,
                    sowing.SownDate);

                if(warning.show)
                {
                    await SendNotificationAsync(
                        $"{sowing.PlantName} behöver uppmärksamhet", warning.message);

                    await Task.Delay(500);
                }
            }
        }
    
        
    
    }
}
