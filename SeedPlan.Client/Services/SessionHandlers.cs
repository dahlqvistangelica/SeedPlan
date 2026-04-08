using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

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
    /// Provides in-memory session persistence for the current application instance using local storage semantics.
    /// </summary>
    /// <remarks>This handler stores the session only for the lifetime of the current application process. Sessions
    /// are not persisted across application restarts or shared between different instances. This implementation is suitable
    /// for scenarios where session data does not need to be durable or shared.</remarks>
    public class LocalStorageSessionHandler : IGotrueSessionPersistence<Session>
    {
        private Session? _session;
        public void SaveSession(Session session) => _session = session;
        public void DestroySession() => _session = null;
        public Session? LoadSession() => _session;

    }
}
