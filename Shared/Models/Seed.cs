using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace SeedPlan.Shared.Models
{
    [Table("seeds")] // This maps the class to the "seeds" table in Supabase
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

        [Column("expiry_date")] // Supabase often uses snake_case
        public DateTime? ExpiryDate { get; set; }

        [Column("purchase_date")]
        public DateTime? PurchaseDate { get; set; }

        [Column("purchase_location")]
        public string? PurchaseLocation { get; set; }

        [Column("germination_rate")]
        public int? GerminationRate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("user_id")] // To link the seed to a specific user
        public string UserId { get; set; } = string.Empty;

        // We rename these so the library does not auto-generate "!inner"
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
            PurchaseDate = old.PurchaseDate;
            PurchaseLocation = old.PurchaseLocation;
            GerminationRate = old.GerminationRate;
            Notes = old.Notes;
            UserId = old.UserId;
            PlantData = old.PlantData;
            VarietyData = old.VarietyData;
        }
    }
}
