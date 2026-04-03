using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace SeedPlan.Shared.Models
{
    [Table("user_profiles")]
    public class UserProfile: BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;
        [Column("full_name")]
        public string? FullName { get; set; }
        [Column("last_frost_date")]
        public DateTime? LastFrostDate { get; set; }
        [Column("growing_zone")]
        public int GrowingZone { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedLast { get; set; }
        [Column("preferred_plant_categories")]
        public int[] PreferredPlantCategories { get; set; } = [];

        // -- WEATHER WARNING FIELDS --

        [Column("city")]
        public string? City { get; set; }
        [Column("latitude")]
        public double? Latitude { get; set; }
        [Column("longitude")]
        public double? Longitude { get; set; }

        [Column("is_admin")]
        public bool IsAdmin { get; set; } = false;

    }
}
