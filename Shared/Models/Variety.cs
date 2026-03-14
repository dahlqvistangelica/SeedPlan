using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace SeedPlan.Shared.Models
{
    [Table("varieties")]
    public class Variety : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("plant_id")]
        public int PlantId { get; set; }

        [Column("variety_name")]
        public string VarietyName { get; set; } = string.Empty;
    }
}
