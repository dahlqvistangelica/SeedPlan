using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Microsoft.JSInterop;
using System.Text.Json;

namespace SeedPlan.Client.Services
{

        /// <summary>
        /// Provides an in-memory implementation of session persistence for storing, retrieving, and destroying a single
        /// session instance.
        /// </summary>
        /// <remarks>This class is intended for scenarios where session data does not need to be persisted
        /// beyond the application's lifetime, such as testing or simple in-memory workflows. It is not thread-safe and
        /// should not be used in multi-threaded or distributed environments where session state must be shared or
        /// persisted.</remarks>
        public class InMemorySessionHandler : IGotrueSessionPersistence<Session>
        {
            private Session? _session;
            public void SaveSession(Session session) => _session = session;
            public void DestroySession() => _session = null;
            public Session? LoadSession() => _session;
        }

    /// <summary>
    /// Provides session persistence using the browser's local storage for Gotrue authentication sessions.
    /// </summary>
    /// <remarks>This class enables storing, retrieving, and removing authentication session data in the
    /// browser's local storage via JavaScript interop. It is intended for use in Blazor applications where session
    /// state needs to persist across browser reloads or tabs. Thread safety is not guaranteed; concurrent access from
    /// multiple threads may result in inconsistent state.</remarks>
    public class LocalStorageSessionHandler : IGotrueSessionPersistence<Session>
    {
        private Session? _session;
        public void SaveSession(Session session) => _session = session;
        public void DestroySession() => _session = null;
        public Session? LoadSession() => _session;
        /*
        private readonly IJSInProcessRuntime? _js;
        public LocalStorageSessionHandler(IJSInProcessRuntime? js) { _js = js; }

        public void SaveSession(Session session) =>
            _js?.InvokeVoid("localStorage.setItem", "sb_session", JsonSerializer.Serialize(session));

        public void DestroySession() =>
            _js?.InvokeVoid("localStorage.removeItem", "sb_session");

        public Session? LoadSession()
        {
            if (_js == null) return null;
            try
            {
                var json = _js.Invoke<string>("localStorage.getItem", "sb_session");
                return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Session>(json);
            }
            catch { return null; }
        }*/
        
    }
    }
