using System.Text.Json.Serialization;

namespace SeedPlan.Shared.Models
{
    // This class exactly matches how the data looks in Supabase "user_metadata"
    public class UserMetaData
    {
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }
}