using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeedPlan.Shared.Models
{
    [Table("app_features")]
    public class AppFeature: BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("message")]
        public string Message { get; set; }
        [Column("version_tag")]
        public string VersionTag { get; set; }
    }
}
