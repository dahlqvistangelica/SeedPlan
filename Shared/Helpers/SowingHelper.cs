using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;

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
                3 => days > 14 ? (true, days, $"Omskolad för {days} dagar sedan — dags att börja avhärda?") : (false, 0, ""),
                4 => days > 14 ? (true, days, $"Avhärdas sedan {days} dagar — redo att plantera ut?") : (false, 0, ""),
                5 => days > 45 ? (true, days, $"Utplanterad sedan {days} dagar — dags att registrera skörd?") : (false, 0, ""),
                _ => (false, 0, "")
            };
        }

        public static(bool show, int days, string message) GetStaleWarning(int status, DateTime? statusUpdatedAt, DateTime sownDate, NotificationSettings settings)
        {
            if(settings == null || !settings.Enabled || !settings.SowingNotificationsEnabled)
            {
                return (false, 0, "");
            }
            var lastUpdate = statusUpdatedAt ?? sownDate;
            var days = (DateTime.UtcNow - lastUpdate).Days;

            return status switch
            {
                0 => days > settings.DaysSownReminder ? (true, days, $"Har inte grott på {days} dagar — groplats, fukt och ljus rätt?") : (false, 0, ""),
                1 => days > settings.DaysGerminatedReminder
    ? (true, days, $"Har stått på Grodd i {days} dagar — dags att kolla karaktärsblad?")
    : (false, 0, ""),

                2 => days > settings.DaysTrueLeavesReminder
                    ? (true, days, $"Har haft karaktärsblad i {days} dagar — dags att omskola?")
                    : (false, 0, ""),

                3 => days > settings.DaysPottedOnReminder
                    ? (true, days, $"Omskolad för {days} dagar sedan — dags att börja avhärda?")
                    : (false, 0, ""),

                4 => days > settings.DaysHardeningOffReminder
                    ? (true, days, $"Avhärdas sedan {days} dagar — redo att plantera ut?")
                    : (false, 0, ""),

                5 => days > settings.DaysPlantedOutReminder
                    ? (true, days, $"Utplanterad sedan {days} dagar — dags att registrera skörd?")
                    : (false, 0, ""),

                _ => (false, 0, "")
            };
        }

        public static(bool show, string message) GetPlantOutWarning(SowingView sowing, DateTime sownDate)
        {

            var days = (DateTime.UtcNow - sownDate).Days;
            var sowingLeadTimeMin = sowing.SowingLeadTimeMin * 7;
            var sowingLeadTimeMax = sowing.SowingLeadTimeMax * 7;
            if (days > sowingLeadTimeMin && days < sowingLeadTimeMax)
            {
                return (true, $"{sowing.PlantName} har växt tillräckligt länge för att planteras ut nu!");
            }
            if(days > sowingLeadTimeMax)
            {
                return (true, $"{sowing.PlantName} kunde planteras ut för {days - sowingLeadTimeMax} dagar sen.");
            }
            if(days < sowingLeadTimeMin)
            {
                return (true, $"{sowing.PlantName} kan planteras ut om {sowingLeadTimeMin - days} dagar");
            }
            return (false, "");

        }
    }
}