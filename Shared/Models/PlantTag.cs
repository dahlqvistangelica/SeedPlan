using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    [Table("tags")]
    public class PlantTag : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("tag_key")]
        public string TagKey { get; set; } = string.Empty;

        [Column("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [Column("sort_order")]
        public int SortOrder { get; set; }
    }
}