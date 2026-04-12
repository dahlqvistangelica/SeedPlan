using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Shared.Models
{
    [Table("user_seen_features")]
    public class UserSeenFeature : BaseModel
    {
        [Column("user_id")]
        public string UserId { get; set; }
        [Column("feature_id")]
        public int FeatureId { get; set; }
    }
}
