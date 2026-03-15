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

        [Column("user_id")]
        public string? UserId { get; set; }

        // Denna används för att visa namnet på fröet i gränssnittet
        [Reference(typeof(Seed))]
        public Seed? Seed { get; set; }

        public Sowing() { }
        public Sowing(int sId)
        {
            SeedId = sId;
            Status = 0;
        }
    }
}
