using System.Text.Json.Serialization;

namespace SeedPlan.Shared.Models
{
    // Denna klass matchar exakt hur datat ser ut i Supabase "user_metadata"
    public class UserMetaData
    {
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }
}