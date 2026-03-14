using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace SeedPlan.Shared.Models
{
    [Table("seeds")] // Detta mappar klassen till tabellen "seeds" i Supabase
    public class Seed : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }
        [Column("plant_id")]
        public int? PlantId { get; set; }
        [Column("variety_id")]
        public int? VarietyId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("expiry_date")] // Supabase använder ofta snake_case
        public string? ExpiryDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("user_id")] // För att koppla fröet till en specifik användare
        public string? UserId { get; set; }
        [Reference(typeof(Plant))]
        public Plant? Plant { get; set; }
        [Reference(typeof(Variety))]
        public Variety? Variety { get; set; }
    }
}
