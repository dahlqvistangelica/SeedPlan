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
        public int Id { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("message")]
        public string Message { get; set; }
        [Column("version_tag")]
        public string VersionTag { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; }
        [Column("is_mandatory")]
        public bool IsMandatory { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

    }
}
