using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models.ViewModels
{


    [Table("v_user_inventory")]
    public class SeedView : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("plant_id")]
        public int? PlantId { get; set; }

        [Column("variety_id")]
        public int? VarietyId { get; set; }

        [Column("plant_name")]
        public string? PlantName { get; set; }

        [Column("variety_name")]
        public string? VarietyName { get; set; }
    }
}
