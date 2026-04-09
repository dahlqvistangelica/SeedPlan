using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SeedPlan.Client.Services
{
    public class FeedbackModalService
    {
        private readonly Supabase.Client _supabase;

        public bool IsVisible { get; private set; } = false;

        // Comment translated to English.
        public event EventHandler? OnVisibilityChanged;

        public FeedbackModalService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task SendFeedback(FeedbackModel feedback)
        {
            try
            {
                await _supabase.From<FeedbackModel>().Insert(feedback);
            }
            catch (Supabase.Postgrest.Exceptions.PostgrestException pgEx)
            {
                throw new Exception($"Databasfel: {pgEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Kunde inte skicka meddelandet");
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
        public int Id { get; set; }
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

        public override string ToString()
        {
            return $", Type:{FeedbackType}, Subject:{Subject}, Category:{Category}, Message: {Message}, Email: {Email}";
        }
    }
}
