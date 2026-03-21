using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Models.ViewModels
{
    [Table("v_user_sowings")]
    public class SowingView : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("sown_date")]
        public DateTime SownDate { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("seed_id")]
        public int SeedId { get; set; }

        [Column("seed_name")]
        public string? SeedName { get; set; }

        [Column("plant_name")]
        public string? PlantName { get; set; }

        [Column("variety_name")]
        public string? VarietyName { get; set; }

        [Column("sowing_depth_mm")]
        public int? SowingDepth { get; set; }

        [Column("is_light_germinating")]
        public bool? IsLightGerminating { get; set; }

        [Column("requires_topping")]
        public bool? RequiresTopping { get; set; }
        [Column("status_updated_at")]
        public DateTime? StatusUpdatedAt { get; set; }
    }
}
