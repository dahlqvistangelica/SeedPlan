using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    // ─── Runtime models (UI layer, not persisted directly) ───────────────────

    public class Garden
    {
        public long Id { get; set; }
        public string Name { get; set; } = "Min trädgård";
        public int Year { get; set; } = DateTime.Now.Year;
        public List<CultivationArea> Areas { get; set; } = new();
    }

    public class CultivationArea
    {
        public long Id { get; set; }
        public long GardenId { get; set; }
        public string Name { get; set; } = "Yta 1";
        public int WidthCm { get; set; } = 80;
        public int LengthCm { get; set; } = 120;
        public string Color { get; set; } = "#86efac";
        public int SortOrder { get; set; }
        public List<PlantedCrop> Crops { get; set; } = new();
    }

    public class PlantedCrop
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Plant Plant { get; set; } = null!;
        public double CenterX { get; set; }
        public double CenterY { get; set; }
    }

    // ─── Supabase DB models ───────────────────────────────────────────────────

    [Table("gardens")]
    public class GardenDb : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("year")]
        public int Year { get; set; }

        [Column("inserted_at")]
        public DateTime InsertedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("cultivation_areas")]
    public class GardenPlan : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("garden_id")]
        public long? GardenId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("width_cm")]
        public int WidthCm { get; set; }

        [Column("length_cm")]
        public int LengthCm { get; set; }

        [Column("color")]
        public string Color { get; set; } = "#86efac";

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("inserted_at")]
        public DateTime InsertedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("crops")]
    public class GardenPlanCrop : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("area_id")]
        public long AreaId { get; set; }

        [Column("plant_id")]
        public long PlantId { get; set; }

        [Column("center_x")]
        public double CenterX { get; set; }

        [Column("center_y")]
        public double CenterY { get; set; }

        [Column("inserted_at")]
        public DateTime InsertedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
