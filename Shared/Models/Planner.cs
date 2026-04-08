using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Models
{
    public class CultivationArea
    {
        public string Name { get; set; } = "Min Pallkrage";
        public int WidthCm { get; set; } = 80;  // Standard pallkrage
        public int LengthCm { get; set; } = 120; // Standard pallkrage
        public List<PlantedCrop> Crops { get; set; } = new();
    }

    public class PlantedCrop
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // För att Blazor ska kunna hålla isär dem när vi drar
        public Plant Plant { get; set; }

        // Positionens mittpunkt i centimeter
        public double CenterX { get; set; }
        public double CenterY { get; set; }
    }

    [Table("garden_plans")]
    public class GardenPlan : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } // Vem äger planen?

        [Column("name")]
        public string Name { get; set; } // T.ex. "Pallkrage 1"

        [Column("width_cm")]
        public int WidthCm { get; set; }

        [Column("length_cm")]
        public int LengthCm { get; set; }
    }

    [Table("garden_plan_crops")]
    public class GardenPlanCrop : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("plan_id")]
        public string PlanId { get; set; } // Kopplar till GardenPlan

        [Column("plant_id")]
        public int PlantId { get; set; } // Vilken växt?

        [Column("center_x")]
        public double CenterX { get; set; } // X-koordinat sparad!

        [Column("center_y")]
        public double CenterY { get; set; } // Y-koordinat sparad!
    }
}
