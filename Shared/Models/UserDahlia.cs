using SeedPlan.Shared.Interfaces;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    [Table("user_dahlias")]
    public class UserDahlia : BaseModel, IHasPhoto
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
        [Column("owner_id")]
        public string UserId { get; set; } = string.Empty;
        [Column("variety_id")]
        public string VarietyId { get; set; } = string.Empty;
        [Column("have_qty")]
        public int HaveQty { get; set; } = 0;
        [Column("lost_in_storage")]
        public int LostInStorage { get; set; } = 0;
        [Column("remain_qty")]
        public int RemainingQty { get; set; } = 0;
        [Column("location")]
        public string Location { get; set; } = string.Empty;
        [Column("notes")]
        public string Notes { get; set; } = string.Empty;
        [Column("photo_url")]
        public string? PhotoUrl { get; set; } 
        [Column("updated_at")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("source")]
        public string Source { get; set; } = string.Empty;
        [Column("like_level")]
        public int? Rating { get; set; } = null;
        [Reference(typeof(Dahlia))]
        public Dahlia Variety { get; set; }


    }
}
