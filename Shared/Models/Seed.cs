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
        [Column("variety")]
        public string VarietyName { get; set; } = string.Empty;

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("expiry_date")] // Supabase använder ofta snake_case
        public DateTime? ExpiryDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("user_id")] // För att koppla fröet till en specifik användare
        public string UserId { get; set; } = string.Empty;

        // Vi döper om dessa så att biblioteket inte autogenererar "!inner"
        [Reference(typeof(Plant))]
        public Plant? PlantData { get; set; }

        [Reference(typeof(Variety))]
        public Variety? VarietyData { get; set; }

        public Seed() { }
        public Seed(Seed old)
        {
            Id = old.Id;
            PlantId = old.PlantId;
            VarietyId = old.VarietyId;
            Name = old.Name;
            VarietyName = old.VarietyName;
            Quantity = old.Quantity;
            ExpiryDate = old.ExpiryDate;
            Notes = old.Notes;
            UserId = old.UserId;
            PlantData = old.PlantData;
            VarietyData = old.VarietyData;
        }
    }
}
