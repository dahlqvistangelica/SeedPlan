using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Models.ViewModels
{
    [Table("dahlia_unique_colors")]
    public class DahliaColor : BaseModel
    {
        [Column("color_name")]
        public string ColorName { get; set; }
    }
}
