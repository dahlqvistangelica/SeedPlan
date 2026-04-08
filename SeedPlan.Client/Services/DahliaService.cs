using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.Services
{
    public class DahliaService: IDahliaService
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




    }
}
