using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    [Table("plant_tags")]
    public class PlantTagLink : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("plant_id")]
        public int PlantId { get; set; }

        [Column("tag_id")]
        public int TagId { get; set; }
    }
}