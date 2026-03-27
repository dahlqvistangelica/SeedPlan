using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Threading.Tasks;

namespace SeedPlan.Client.Services
{
    public class FeedbackModalService
    {
        private readonly Supabase.Client _supabase;
        
        public bool IsVisible { get; private set; } = false;

        // Använd C# standard event mönster istället
        public event EventHandler? OnVisibilityChanged;

        public FeedbackModalService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task SendFeedback(FeedbackModel feedback)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user != null)
            {
                 // Ställ in user_id och datum automatiskt här för säkerhets skull
                 feedback.UserId = user.Id;
                 feedback.CreatedAt = DateTime.UtcNow;

                 await _supabase.From<FeedbackModel>().Insert(feedback);
            }
            else 
            {
                throw new Exception("Du måste vara inloggad för att skicka feedback");
            }
        }

        public void Open()
        {
            IsVisible = true;
            OnVisibilityChanged?.Invoke(this, EventArgs.Empty);
            Console.WriteLine("FeedbackModalService.Open() anropad! IsVisible är nu: " + IsVisible);
        }

        public void Close()
        {
            IsVisible = false;
            OnVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [Table("user_feedback")]
    public class FeedbackModel : BaseModel
    {
        [PrimaryKey("id", false)] 
        public long Id { get; set; }

        [Column("feedback_type")]
        public string FeedbackType { get; set; } = "";
        
        [Column("subject")]
        public string Subject { get; set; } = "";
        
        [Column("category")]
        public string Category { get; set; } = "";
        
        [Column("message")]
        public string Message { get; set; } = "";
        
        [Column("email")]
        public string? Email { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("user_id")]
        public string? UserId { get; set; }
    }
}
