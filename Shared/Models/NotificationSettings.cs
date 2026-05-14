using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    [Table("notification_settings")]
    public class NotificationSettings : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("enabled")]
        public bool Enabled { get; set; } = true;

        [Column("days_before_sowing")]
        public int[] DaysBeforeSowing { get; set; } = [7, 2];

        [Column("days_inactive_reminder")]
        public int DaysInactiveReminder { get; set; } = 14;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("reminder_days_sown")]
        public int DaysSownReminder { get; set; } = 10;
        [Column("reminder_days_germinated")]
        public int DaysGerminatedReminder { get; set; } = 14;
        [Column("reminder_days_true_leaves")]
        public int DaysTrueLeavesReminder { get; set; } = 21;
        [Column("reminder_days_potted_on")]
        public int DaysPottedOnReminder { get; set; } = 14;
        [Column("reminder_days_hardening_off")]
        public int DaysHardeningOffReminder { get; set; } = 14;
        [Column("reminder_days_planted_out")]
        public int DaysPlantedOutReminder { get; set; } = 45;
        [Column("sowing_notifications_enabled")]
        public bool SowingNotificationsEnabled { get; set; } = true;
    }
}
