using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    [Table("sowings")]
    public class Sowing : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("seed_id")]
        public int SeedId { get; set; }

        [Column("sown_date")]
        public DateTime? SownDate { get; set; }

        [Column("status")]
        public int Status { get; set; } = 0;

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("status_updated_at")]
        public DateTime? StatusUpdatedAt { get; set; }

        [Column("batch_number")]
        public int BatchNumber { get; set; } = 1;
        // Used for UI-only display of related seed information when needed.
        //[Reference(typeof(Seed))]
        //public Seed? Seed { get; set; }

        public Sowing() { }
        public Sowing(int sId, int quantity)
        {
            SeedId = sId;
            Status = 0;
            Quantity = quantity;
        }
    }
}
