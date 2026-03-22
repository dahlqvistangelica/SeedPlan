using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;


namespace SeedPlan.Shared.Models
{

        [Table("push_subscriptions")]
        public class PushSubscription : BaseModel
        {
            [PrimaryKey("id", false)]
            public int Id { get; set; }

            [Column("user_id")]
            public string UserId { get; set; } = string.Empty;

            [Column("subscription_json")]
            public string SubscriptionJson { get; set; } = string.Empty;

            [Column("updated_at")]
            public DateTime UpdatedAt { get; set; }
        }
    
}
