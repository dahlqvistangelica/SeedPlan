using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    [Table("sowing_events")]
    public class SowingEvent : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
        [Column("sowing_id")]
        public int SowingId { get; set; }
        [Column("user_id")]
        public string UserId { get; set; }
        [Column("event_type")]
        public string EventType { get; set; }
        [Column("event_date")]
        public DateTime EventDate { get; set; }
        [Column("seedlings_count")]
        public int? SeedlingsCount { get; set; }
        [Column("harvest_weight_g")]
        public int? HarvestWeightG { get; set; }
        [Column("harvest_count")]
        public int? HarvestCount { get; set; }
        [Column("notes")]
        public string? Notes { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }



    }
}
