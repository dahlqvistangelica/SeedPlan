using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class DahliaService : IDahliaService
    {
        public readonly Supabase.Client _supabase;


        public DahliaService(Supabase.Client supabase)
        {
            _supabase = supabase;

        }

        public async Task<List<Dahlia>> GetAllDahliasAsync()
        {
            var response = await _supabase
                .From<Dahlia>()
                .Order(d => d.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();
            return response.Models;
        }

        public async Task<List<Dahlia>> SearchDahliasAsync(string searchTerm)
        {
            var response = await _supabase
                .From<Dahlia>()
                .Filter("name", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Order(d => d.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Limit(5) // Begränsa till 15 träffar för att göra det blixtsnabbt
                .Get();

            return response.Models;
        }

        public async Task<Dahlia> AddDahliaVarietyAsync(Dahlia newDahlia)
        {
            var user = _supabase.Auth.CurrentUser;
            if (user != null)
            {
                
                var result = await _supabase.From<Dahlia>().Insert(newDahlia);
                return result.Model;
            }
            else
            {
                throw new Exception("Du måste vara inloggad");
            }
        }



    }
}
