namespace SeedPlan.Shared.Interfaces
{
    public sealed class AuthSignInResult
    {
        public bool HasUser { get; set; }
        public string? AccessToken { get; set; }
        public string? SessionJson { get; set; }
    }

    public interface IAuthClient
    {
        string? CurrentUserEmail { get; }
        Task<AuthSignInResult?> SignIn(string email, string password);
        Task<bool> SignUp(string email, string password, Dictionary<string, object> metadata);
        Task SignOut();
        Task<bool> UpdateEmail(string newEmail);
        Task<bool> UpdatePassword(string newPassword);
    }
}