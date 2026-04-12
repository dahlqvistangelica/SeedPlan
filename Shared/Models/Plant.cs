using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SeedPlan.Shared.Models
{
    [Table("plants")]
    public class Plant : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("plant_name")]
        public string PlantName { get; set; } = string.Empty;

        [Column("scientific_name")]
        public string? ScientificName { get; set; }

        [Column("sowing_lead_time")]
        public int SowingLeadTime { get; set; }

        [Column("is_light_germinating")]
        public bool IsLightGerminating { get; set; }

        [Column("weeks_before_frost")]
        public int WeeksBeforeFrost { get; set; }

        [Column("hardiness_level")]
        public int HardinessLevel { get; set; }

        [Column("requires_topping")]
        public bool RequiresTopping { get; set; } = false;

        [Column("direct_sowing")]
        public bool DirectSowing { get; set; } = false;

        [Column("category")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlantCategory Category { get; set; } = PlantCategory.Flower;

        // --- NULLABLE VALUES BELOW. ---

        [Column("sowing_lead_time_min")]
        public int? SowingLeadTimeMin { get; set; }

        [Column("sowing_depth_mm")]
        public float? SowingDepth { get; set; } 

        [Column("plant_spacing_cm")]
        public int? PlantSpacing { get; set; } 

        [Column("dev_time_min")]
        public int? DevelopDaysMin { get; set; } 

        [Column("dev_time_max")]
        public int? DevelopDaysMax { get; set; } 

        [JsonIgnore]
        public List<PlantTag> Tags { get; set; } = new();
    }
}