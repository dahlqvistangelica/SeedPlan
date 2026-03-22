namespace SeedPlan.Shared.Helpers
{
    public static class SowingHelper
    {
        public static (bool show, int days, string message) GetStaleWarning(int status, DateTime? statusUpdatedAt, DateTime sownDate)
        {
            var lastUpdate = statusUpdatedAt ?? sownDate;
            var days = (DateTime.UtcNow - lastUpdate).Days;

            return status switch
            {
                0 => days > 10 ? (true, days, $"Har inte grott på {days} dagar — groplats, fukt och ljus rätt?") : (false, 0, ""),
                1 => days > 14 ? (true, days, $"Har stått på Grodd i {days} dagar — dags att kolla karaktärsblad?") : (false, 0, ""),
                2 => days > 21 ? (true, days, $"Har haft karaktärsblad i {days} dagar — dags att omskola?") : (false, 0, ""),
                3 or 4 => days > 14 ? (true, days, $"Omskolad för {days} dagar sedan — dags att börja avhärda?") : (false, 0, ""),
                5 => days > 14 ? (true, days, $"Avhärdas sedan {days} dagar — redo att plantera ut?") : (false, 0, ""),
                _ => (false, 0, "")
            };
        }
    }
}