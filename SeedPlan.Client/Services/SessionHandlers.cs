using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Microsoft.JSInterop;
using System.Text.Json;

namespace SeedPlan.Client.Services
{

        // 1. För Servern (undviker filkrascher och delade inloggningar)
        public class InMemorySessionHandler : IGotrueSessionPersistence<Session>
        {
            private Session? _session;
            public void SaveSession(Session session) => _session = session;
            public void DestroySession() => _session = null;
            public Session? LoadSession() => _session;
        }

        // 2. För Klienten (sparar inloggningen i webbläsarens minne)
        public class LocalStorageSessionHandler : IGotrueSessionPersistence<Session>
        {
            private readonly IJSInProcessRuntime _js;
            public LocalStorageSessionHandler(IJSInProcessRuntime js) { _js = js; }

            public void SaveSession(Session session) =>
                _js.InvokeVoid("localStorage.setItem", "supabase_auth", JsonSerializer.Serialize(session));

            public void DestroySession() =>
                _js.InvokeVoid("localStorage.removeItem", "supabase_auth");

            public Session? LoadSession()
            {
                try
                {
                    var json = _js.Invoke<string>("localStorage.getItem", "supabase_auth");
                    return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Session>(json);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
