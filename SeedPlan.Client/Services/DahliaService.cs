using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;
using Supabase;

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

        public async Task<(List<Dahlia> Dahlias, int TotalCount)> GetPagedDahliasAsync(
    int pageNumber,
    int pageSize,
    string searchTerm = "",
    List<DahliaType>? activeTypes = null,
    List<DahliaSize>? activeSizes = null,
    List<string>? activeColors = null,
    int? maxHeight = null,
    int? minHeight = null)
        {
            // --- LOKAL FUNKTION SOM BYGGER FILTREN ---
            // Comment translated to English.
            Supabase.Postgrest.Interfaces.IPostgrestTable<Dahlia> BuildQuery()
            {
                Supabase.Postgrest.Interfaces.IPostgrestTable<Dahlia> q = _supabase.From<Dahlia>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    q = q.Filter(d => d.Name, Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%");

                if (minHeight.HasValue)
                    q = q.Filter(d => d.Height, Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, minHeight.Value);

                if (maxHeight.HasValue)
                    q = q.Filter(d => d.Height, Supabase.Postgrest.Constants.Operator.LessThanOrEqual, maxHeight.Value);

                if (activeTypes != null && activeTypes.Any())
                    q = q.Filter(d => d.Type, Supabase.Postgrest.Constants.Operator.In, activeTypes);

                if (activeSizes != null && activeSizes.Any())
                    q = q.Filter(d => d.Size, Supabase.Postgrest.Constants.Operator.In, activeSizes);

                if (activeColors != null && activeColors.Any())
                {
                    var filterList = new List<Supabase.Postgrest.Interfaces.IPostgrestQueryFilter>();
                    foreach (var c in activeColors)
                    {
                        // Comment translated to English.
                        filterList.Add(new Supabase.Postgrest.QueryFilter("color", Supabase.Postgrest.Constants.Operator.ILike, $"%{c}%"));
                    }
                    q = q.Or(filterList);
                }

                return q;
            }
            // -----------------------------------------

            // Comment translated to English.
            var countQuery = BuildQuery();
            var totalCount = await countQuery.Count(Supabase.Postgrest.Constants.CountType.Exact);

            // Comment translated to English.
            var dataQuery = BuildQuery();
            var offSet = (pageNumber - 1) * pageSize;

            var response = await dataQuery
                .Order(d => d.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Range(offSet, offSet + pageSize - 1)
                .Get();

            return (response.Models, totalCount);
        }


        public async Task<List<Dahlia>> SearchDahliasAsync(string searchTerm)
        {
            var response = await _supabase
                .From<Dahlia>()
                .Filter("name", Supabase.Postgrest.Constants.Operator.ILike, $"%{searchTerm}%")
                .Order(d => d.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Limit(5) // Comment translated to English.
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

        public async Task<List<string>> GetAvailableColorsAsync()
        {
            var response = await _supabase
                .From<DahliaColor>()
                .Get();

            // Comment translated to English.
            return response.Models.Select(c => c.ColorName).ToList();
        }


    }
}
